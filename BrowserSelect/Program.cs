using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrowserSelect.Properties;
using System.Web;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Data;

namespace BrowserSelect
{
    static class Program
    {
        public static Uri uri = null;
        public static HttpWebRequest webRequestThread = null;
        public static bool uriExpanderThreadStop = false;
        public static string currentVer = "";
        public static string latestVer = "";
        public static (string name, string domain)[] defaultUriExpander = new(string name, string domain)[]
            {
                ("Outlook safe links", "safelinks.protection.outlook.com")//,
                //("Test1", "test.com"),
                //("Test2", "test2.com")
            };

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // to prevent loss of settings when on update
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                // to prevent nullreference in case settings file did not exist
                if (Settings.Default.HideBrowsers == null)
                    Settings.Default.HideBrowsers = new StringCollection();
                if (Settings.Default.AutoBrowser == null)
                    Settings.Default.AutoBrowser = new StringCollection();
                if (!Settings.Default.URLShortners.Contains("*.sendgrid.net"))
                    Settings.Default.URLShortners.Add("*.sendgrid.net");

                Settings.Default.Save();
            }
            currentVer = ((Func<String, String>)((x) => x.Substring(0, x.Length - 2)))(Application.ProductVersion);
            //load URL Shortners
            string[] defultUrlShortners = new string[] {
                "adf.ly",
                "bit.do",
                "bit.ly",
                "goo.gl",
                "ht.ly",
                "is.gd",
                "ity.im",
                "lnk.co",
                "ow.ly",
                "q.gs",
                "rb.gy",
                "rotf.lol",
                "t.co",
                "tiny.one",
                "tinyurl.com"
            };
            if (Settings.Default.URLShortners == null)
            {
                StringCollection url_shortners = new StringCollection();
                url_shortners.AddRange(defultUrlShortners);
                Settings.Default.URLShortners = url_shortners;
                Settings.Default.Save();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Task taskForm = null;

            //checking if a url is being opened or app is ran from start menu (without arguments)
            bool loadedBrowser = false;
            uri = null;

            if (args.Length == 0)
            {
                try
                {
                    uri = new Uri(Clipboard.GetText());
                }
                catch
                {
                    uri = null;
                }
            }
            else
            {
                //Parse url, and add http:// to url if it is missing a protocol
                uri = !string.IsNullOrEmpty(args[0]) ? new UriBuilder(args[0]).Uri : null;
            }
            
            if (uri != null)
            {
                //check to see if auto select rules match
                uri = UriExpander(uri);
                if (Settings.Default.ExpandUrl != null && Settings.Default.ExpandUrl != "Never")
                    uri = UriFollowRedirects(uri);
            }

            // Only run the rules if the url was passed as an argument
            if (args.Length > 0 && uri != null)
            {
                //if we loaded the browser finish execution here...
                loadedBrowser = load_browser(uri);
            }

            Form form = null;
            if (!loadedBrowser)
            {
                // display main form
                if (uri == null && (Boolean)Settings.Default.LaunchToSettings)
                    form = new frm_settings();
                else
                    form = new Form1();
                taskForm = Task.Factory.StartNew(() =>
                {
                    Application.Run(form);
                });
            }
            // check for update
            //if update is available show update icon if Form1 is displayed otherwise for popup
            if (CheckForUpdates())
                if (form is Form1 form1 && !form1.Disposing && !form1.IsDisposed)
                    form1.DisplayUpdate();
                else
                    UpdateDialog();

            if (taskForm != null)
                taskForm.Wait();
            
            System.Diagnostics.Debug.WriteLine("END");
        }

        private static Boolean load_browser(Uri uri)
        {
            if (Settings.Default.Rules != null && Settings.Default.Rules != "")
            {
                DataTable rules = (DataTable)JsonConvert.DeserializeObject(Settings.Default.Rules, (typeof(DataTable)));
                foreach (DataRow rule in rules.Rows)
                {
                    Boolean rule_match = false;
                    string match_type = (string)rule["Type"];
                    string match = (string)rule["Match"];
                    string pattern = (string)rule["Pattern"];

                    string test_uri = "";
                    if (match == "Domain")
                        test_uri = uri.Authority;
                    else if (match == "URL Path")
                        test_uri = uri.PathAndQuery;
                    else if (match == "Full URL")
                        test_uri = uri.AbsoluteUri;

                    switch (match_type)
                    {
                        case "Ends With":
                            if (test_uri.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))
                                rule_match = true;
                            break;
                        case "Starts With":
                            if (test_uri.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                                rule_match = true;
                            break;
                        case "Contains":
                            if (test_uri.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                                rule_match = true;
                            break;
                        case "Matches":
                            if (test_uri.Equals(pattern, StringComparison.OrdinalIgnoreCase))
                                rule_match = true;
                            break;
                        case "RegEx":
                            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                            if (regex.IsMatch(test_uri))
                                rule_match = true;
                            break;
                    }

                    if (rule_match)
                    {
                        Thread.Sleep(500);
                        var alt = Keyboard.IsKeyDown(Keys.Menu); // Alt key
                        if (alt)
                            return false;

                        System.Diagnostics.Debug.WriteLine(test_uri + " " + match_type + " " + pattern);
                        string browser = (string)rule["Browser"];
                        if (browser != "display BrowserSelect")
                            Form1.open_url((Browser)browser, false, false);
                        return true;
                    }
                }
            }
            if (Settings.Default.DefaultBrowser != null &&
                Settings.Default.DefaultBrowser != "" &&
                Settings.Default.DefaultBrowser != "display BrowserSelect")
            {
                Thread.Sleep(500);
                var alt = Keyboard.IsKeyDown(Keys.Menu); // Alt key
                if (alt)
                    return false;

                Form1.open_url((Browser)Settings.Default.DefaultBrowser, false, false);
                return true;
            }
            return false;
        }

        public static string Args2Str(List<string> args)
        {
            return Args2Str(args.ToArray());
        }
        public static string Args2Str(string[] args)
        {
            return string.Join(" ", args.Select(Program.EncodeParameterArgument));
        }

        /// <summary>
        /// Encodes an argument for passing into a program
        /// taken from : http://stackoverflow.com/a/12364234/1461004
        /// </summary>
        /// <param name="original">The value that should be received by the program</param>
        /// <returns>The value which needs to be passed to the program for the original value 
        /// to come through</returns>
        public static string EncodeParameterArgument(string original)
        {
            if (string.IsNullOrEmpty(original))
                return original;
            string value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
            value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"");
            return value;
        }

        // http://stackoverflow.com/a/194223/1461004
        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private static Uri UriExpander(Uri uri)
        {
            List<string> enabled_url_expanders = new List<string>();
            if (Settings.Default.URLProcessors != null)
            {
                foreach ((string name, string domain) in defaultUriExpander)
                {
                    if (Settings.Default.URLProcessors.Contains(name))
                    {
                        enabled_url_expanders.Add(domain);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("URLExpander: " + uri.Authority);
            if (uri.Authority.EndsWith("safelinks.protection.outlook.com") &&
                enabled_url_expanders.Contains("safelinks.protection.outlook.com"))
            {
                var queryDict = HttpUtility.ParseQueryString(uri.Query);
                if (queryDict != null && queryDict.Get("url") != null)
                {
                    uri = new UriBuilder(HttpUtility.UrlDecode(queryDict.Get("url"))).Uri;
                }
            }

            return uri;
        }

        private static Uri UriFollowRedirects(Uri uri, int num_redirects = 0)
        {
            int max_redirects = 20;
            if (num_redirects >= max_redirects)
            {
                return uri;
            }
            System.Diagnostics.Debug.WriteLine("Url " + num_redirects + " " + uri.Authority);
            var url_shortners = Settings.Default.URLShortners.Cast<string>();
            Form SplashScreen = null;

            var shouldMakeRequest = (Settings.Default.ExpandUrl == "Follow all redirects") ||
                url_shortners.Where(u => WildcardMatches(u, uri.Authority)).Any();

            if (!Program.uriExpanderThreadStop && shouldMakeRequest)
            {
                //Thread.Sleep(2000);
                if (num_redirects == 0)
                {
                    SplashScreen = new frm_SplashScreen();
                    var splashThread = new Thread(new ThreadStart(() => Application.Run(SplashScreen)));
                    splashThread.Start();
                }
                HttpWebResponse response = MyWebRequest(uri);
                if (response != null)
                {
                    if ((int)response.StatusCode > 299 && (int)response.StatusCode < 400)
                    {
                        uri = UriFollowRedirects(new UriBuilder(response.Headers["Location"]).Uri, (num_redirects + 1));
                    }
                    else
                    {
                        uri = response.ResponseUri;
                    }
                }
            }

            if (num_redirects == 0)
            {
                if (SplashScreen != null && !SplashScreen.Disposing && !SplashScreen.IsDisposed)
                    try
                    {
                        Program.uriExpanderThreadStop = true;
                        SplashScreen.Invoke(new Action(() => SplashScreen.Close()));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
            }
            return uri;
        }

        public static Boolean CheckForUpdates()
        {
            if (Settings.Default.CheckForUpdates)
            {
                DateTime lastUpdateCheck;
                // On first load LastUpdateCheck is not set, Checking for a null value didn't work
                // So if we fail to load it, we'll set it to 1980 to force an update check
                try
                {
                    lastUpdateCheck = Settings.Default.LastUpdateCheck;
                }
                catch
                {
                    lastUpdateCheck = new DateTime(1980, 1, 1, 0, 0, 0);
                }
                System.Diagnostics.Debug.WriteLine("lastUpdateCheck: " + lastUpdateCheck + " diff, days: " + (DateTime.Now.Subtract(lastUpdateCheck)).Days);
                if ((DateTime.Now - lastUpdateCheck).Days >= 1)
                {
                    Task taskCheckUpdate = QueryUpdates();
                    taskCheckUpdate.Wait();
                    Settings.Default.LastUpdateCheck = DateTime.Now;
                    Settings.Default.Save();
                }
                if (latestVer == "")
                    latestVer = Settings.Default.LatestVersion;
                //update available?
                if (latestVer != "" && currentVer.CompareTo(latestVer) < 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static Task QueryUpdates()
        {
            if (currentVer == "")
                currentVer = ((Func<String, String>)((x) => x.Substring(0, x.Length - 2)))(Application.ProductVersion);
            Task task = Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Debug.WriteLine("QueryUpdates");
                // request to releases/latest redirects the user to /releases/tag.
                // since tag is the version number we can get latest version from Location header
                // and make a HEAD request instead of get to save bandwidth
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create($"https://github.com/{Settings.Default.GithubRepo}/releases/latest");
                ServicePointManager.Expect100Continue = true;
                webRequest.Method = "HEAD";
                webRequest.AllowAutoRedirect = false;
                webRequest.Timeout = 2000;
                WebResponse response = null;
                try
                {
                    response = webRequest.GetResponse();
                }
                catch (WebException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                if (response != null)
                {
                    if (response.Headers["Location"] != null)
                    {
                        string locationResponse = response.Headers["Location"].Split('/').Last();
                        locationResponse = locationResponse.Split('_').First();
                        bool isNumeric = true;
                        foreach (var num in locationResponse.Split('.'))
                        {
                            if (!int.TryParse(num, out _))
                                isNumeric = false;
                        }
                        if (isNumeric)
                        {
                            latestVer = locationResponse;
                            Settings.Default.LatestVersion = latestVer;
                            Settings.Default.Save();
                            #if DEBUG
                            int verCompare = currentVer.CompareTo(latestVer);
                            if (verCompare > 0)
                                System.Diagnostics.Debug.WriteLine("Current version: " + currentVer + " is greater than, latest published version: " + latestVer);
                            else if (verCompare < 0)
                                System.Diagnostics.Debug.WriteLine("Current version: " + currentVer + " is less than, latest published version: " + latestVer);
                            else
                                System.Diagnostics.Debug.WriteLine("Current version: " + currentVer + " equals, latest published version: " + latestVer);
                            #endif
                        }
                        else
                        {
                            latestVer = "";
                            System.Diagnostics.Debug.WriteLine("Error converting version number...");
                        }
                    }
                    else
                    {
                        latestVer = "";
                        System.Diagnostics.Debug.WriteLine("Error retrieving version number... no location header in response.");
                    }
                    response.Close();
                }
                else
                {
                    latestVer = "";
                    System.Diagnostics.Debug.WriteLine("Error retrieving version number... response null.");
                }
            });
            return task;
        }

        public static void UpdateDialog()
        {
            DialogResult dUpdate = MessageBox.Show(String.Format(
              "New Update Available!\nCurrent Version: {0}\nLast Version: {1}" +
              "\nto Update download and install the new version from project's github." +
              "\nDo you want to download the update now?",
              currentVer, latestVer), "Updates Avaialble", MessageBoxButtons.YesNo);
            if (dUpdate == DialogResult.Yes)
                System.Diagnostics.Process.Start($"https://github.com/{Settings.Default.GithubRepo}/releases/latest");
        }

        private static HttpWebResponse MyWebRequest(Uri uri)
        {
            //Support TLS1.2 - updated .Net framework - no longer needed
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Ssl3; //SecurityProtocolType.Tls12;
            var webRequest = (HttpWebRequest)WebRequest.Create(uri.AbsoluteUri);
            // Set timeout - needs to be high enough for HTTP request to succeed on slow network connections,
            // but fast enough not to slow down BrowserSelect startup too much.
            // 2 seconds seems about right
            webRequest.Timeout = 2000;
            //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv: 85.0) Gecko/20100101 Firefox/85.0";
            webRequest.AllowAutoRedirect = false;
            HttpWebResponse response = null;
            try
            {
                var ar = webRequest.BeginGetResponse(null, null);
                Program.webRequestThread = webRequest;
                ThreadPool.RegisterWaitForSingleObject(ar.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), webRequest, webRequest.Timeout, true);
                response = (HttpWebResponse)webRequest.EndGetResponse(ar);
                response.Close();
            }
            catch (WebException ex)
            {
                // We are mostly catch up webRequest.Abort() or webRequest errors here (e.g. untrusted certificates)
                // No action required.
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return response;
        }

        // Abort the request if the timer fires.
        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    System.Diagnostics.Debug.WriteLine("Timed out, aborting HTTP request...");
                    request.Abort();
                }
            }
        }

        private static bool WildcardMatches(string wildcardPattern, string value)
        {
            var p = Regex.Escape(wildcardPattern);

            p = p.Replace("\\*\\.", ".*");
            p = p.Replace("\\.\\*", ".*");
            p = p.Replace("\\*", ".*");

            return Regex.IsMatch(value, p);
        }
    }
}
