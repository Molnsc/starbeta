using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Drawing;
using Newtonsoft.Json;
using System.Security.Cryptography;



namespace StarlightLite
{
    public partial class Lite : Form
    {

        private string _webSocketUrl = null;
        private WebSocket _webSocket = null;
        private bool dragging = false;  // Flag to track if the form is being dragged
        private Point startPoint = new Point(0, 0);
        public string debugURL = "";
        public string authHD = "none";
        public string globalcid = "";
        public string globalgid = "";
        public bool attached = false;

        public Lite()
        {
            InitializeComponent();
            CheckForPID.Start();
      
            isInject.Start();
        }



        private void CloseOpenCmdProcesses()
        {
            var cmdProcesses = Process.GetProcessesByName("cmd");

            foreach (var process in cmdProcesses)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    // Handle any errors if we cannot kill a process (e.g., permission issues)

                }
            }


        }

        private void KillDiscordProcesses()
        {
            var discordProcesses = Process.GetProcessesByName("Discord");

            if (discordProcesses.Length > 0)
            {
                foreach (var process in discordProcesses)
                {
                    try
                    {
                        process.Kill();  // Kill each Discord process
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors if we cannot kill a process (e.g., permission issues)

                    }
                }

            }
            else
            {

            }

        }

        private async void LaunchDiscordWithDebugging()
        {
           
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string discordFolderPath = Path.Combine(appDataPath, "Discord");

            if (Directory.Exists(discordFolderPath))
            {
      
                var appFolders = Directory.GetDirectories(discordFolderPath);
                string discordAppFolder = appFolders.FirstOrDefault(f => f.Contains("app"));

                if (!string.IsNullOrEmpty(discordAppFolder))
                {
                   
                    string cmdCommand = $"/C cd \"{discordAppFolder}\" && discord.exe --remote-debugging-port=9222";

            
                    Process.Start("cmd.exe", cmdCommand);

                    
                    await Task.Delay(3000);

                    string websocketDebuggerUrl = await GetWebSocketDebuggerUrl("http://127.0.0.1:9222/json");

                    if (!string.IsNullOrEmpty(websocketDebuggerUrl))
                    {
                        debugURL = websocketDebuggerUrl;
                    }
                    else
                    {
                        MessageBox.Show("Invalid Debugger URL not found.");
                    }
                }
                else
                {
                    MessageBox.Show("Discord app folder not found.");
                }
            }
            else
            {
                MessageBox.Show("Discord folder not found in AppData.");
            }
        }

        private async Task<string> GetWebSocketDebuggerUrl(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Send a GET request to the remote debugging endpoint
                    string response = await client.GetStringAsync(url);

                    // Parse the JSON response
                    var jsonArray = JArray.Parse(response);

                    // Extract the WebSocket debugger URL (first item in the JSON array)
                    var websocketDebuggerUrl = jsonArray[0]["webSocketDebuggerUrl"]?.ToString();

                    return websocketDebuggerUrl;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching DEBUGGER URL: {ex.Message}");
                return null;
            }
        }


        private void Lite_Load(object sender, EventArgs e)
        {

        }

        private void CheckForPID_Tick(object sender, EventArgs e)
        {
            string appName = "Discord"; // Or your app name
            Process[] processes = Process.GetProcessesByName(appName);

            if (processes.Length > 0)
            {
                string pidInfo = "Active-Process: \n";
                foreach (var process in processes)
                {
                    pidInfo += $"PID: {process.Id}, Name: {process.ProcessName}\n";
                }

                pidfetch.Text = pidInfo; // Display PID info in a TextBox (txtProcessInfo is assumed to be a TextBox)
            }
            else
            {
                pidfetch.Text = "Waiting for discord...";
            }
            CheckForPID.Start();
        }


        public void LoadStarlightAPI()
        {
            var cid = globalcid;
            var gid = globalgid;
            var auth = authHD;
            string mainAPI = @"  
  // Call this to update `cid` and `gid` to current channel and guild id
  var update_guildId_and_channelId_withCurrentlyVisible = (log = true) => {
    gid = window.location.href.split('/').slice(4)[0]
    cid = window.location.href.split('/').slice(4)[1]
    if (log) {
      console.log(`\`gid\` was set to the guild id you are currently looking at (${gid})`)
      console.log(`\`cid\` was set to the channel id you are currently looking at (${cid})`)
    }
  }
  var id = update_guildId_and_channelId_withCurrentlyVisible

  /** @type {import('./types').api['delay']} */
  var delay = ms => new Promise(res => setTimeout(res, ms))
  // prettier-ignore
  var qs = obj => Object.entries(obj).map(([k, v]) => `${k}=${v}`).join('&')

  /** @type {import('./types').api['apiCall']} */
  var apiCall = (apiPath, body, method = 'GET', options = {}) => {
    if (!authHeader) throw new Error(""The authorization token is missing. Did you forget to set it? `authHeader = 'your_token'`"")

    const fetchOptions = {
      body: body ? body : undefined,
      method,
      headers: {
        Accept: '*/*',
        'Accept-Language': 'en-US',
        Authorization: authHeader,
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/1.0.9015 Chrome/108.0.5359.215 Electron/22.3.12 Safari/537.36',
        'X-Super-Properties': btoa(
          JSON.stringify({
            os: 'Windows',
            browser: 'Discord Client',
            release_channel: 'stable',
            client_version: '1.0.9163',
            os_version: '10.0.22631',
            os_arch: 'x64',
            app_arch: 'x64',
            system_locale: 'en-US',
            browser_user_agent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) discord/1.0.9163 Chrome/124.0.6367.243 Electron/30.2.0 Safari/537.36',
            browser_version: '30.2.0',
            os_sdk_version: '22631',
            client_build_number: 327338,
            native_build_number: 52153,
            client_event_source: null,
          }),
        ),
      },
      ...options,
    }
    const isFormData = body?.constructor?.name === 'FormData'
    if (!isFormData) {
      fetchOptions.headers['Content-Type'] = 'application/json'
      fetchOptions.body = JSON.stringify(body)
    }
    return fetch(`https://discord.com/api/v9${apiPath}`, fetchOptions)
      .then(res => {
        if (res.ok) return res.json()
        throw new Error(`Failed to fetch: ${res.status} ${res.statusText}`)
      })
      .catch(err => {
        console.error(err)
        throw new Error('An error occurred while fetching the API.')
      })
  }

  /** @type {import('./types').api} */
  var api = {
    getMessages: (channelOrThreadId, limit = 100, params = {}) => apiCall(`/channels/${channelOrThreadId}/messages?limit=${limit ?? 100}&${qs(params)}`),
    sendMessage: (channelOrThreadId, message, tts, body = {}) => apiCall(`/channels/${channelOrThreadId}/messages`, { content: message, tts: !!tts, ...body }, 'POST'),
    replyToMessage: (channelOrThreadId, repliedMessageId, message, tts, body = {}) =>
      apiCall(`/channels/${channelOrThreadId}/messages`, { content: message, message_reference: { message_id: repliedMessageId }, tts: !!tts, ...body }, 'POST'),
    editMessage: (channelOrThreadId, messageId, newMessage, body = {}) => apiCall(`/channels/${channelOrThreadId}/messages/${messageId}`, { content: newMessage, ...body }, 'PATCH'),
    deleteMessage: (channelOrThreadId, messageId) => apiCall(`/channels/${channelOrThreadId}/messages/${messageId}`, null, 'DELETE'),

    createThread: (channelId, toOpenThreadInmessageId, name, autoArchiveDuration = 1440, body = {}) =>
      apiCall(`/channels/${channelId}/messages/${toOpenThreadInmessageId}/threads`, { name, auto_archive_duration: autoArchiveDuration, location: 'Message', type: 11, ...body }, 'POST'),
    createThreadWithoutMessage: (channelId, name, autoArchiveDuration = 1440, body = {}) =>
      apiCall(`/channels/${channelId}/threads`, { name, auto_archive_duration: autoArchiveDuration, location: 'Message', type: 11, ...body }, 'POST'),
    deleteThread: threadId => apiCall(`/channels/${threadId}`, null, 'DELETE'),

  
    sendEmbed: (channelOrThreadId, embed = { title: 'Title', description: 'Description' }) => apiCall(`/channels/${channelOrThreadId}/messages`, { embed }, 'POST'),

    getRoles: guildId => apiCall(`/guilds/${guildId}/roles`),
    createRole: (guildId, name) => apiCall(`/guilds/${guildId}/roles`, { name }, 'POST'),
    deleteRole: (guildId, roleId) => apiCall(`/guilds/${guildId}/roles/${roleId}`, null, 'DELETE'),

    getBans: guildId => apiCall(`/guilds/${guildId}/bans`),
    banUser: (guildId, userId, reason) => apiCall(`/guilds/${guildId}/bans/${userId}`, { delete_message_days: '7', reason }, 'PUT'),
    unbanUser: (guildId, userId) => apiCall(`/guilds/${guildId}/bans/${userId}`, null, 'DELETE'),
    kickUser: (guildId, userId) => apiCall(`/guilds/${guildId}/members/${userId}`, null, 'DELETE'),

    addRole: (guildId, userId, roleId) => apiCall(`/guilds/${guildId}/members/${userId}/roles/${roleId}`, null, 'PUT'),
    removeRole: (guildId, userId, roleId) => apiCall(`/guilds/${guildId}/members/${userId}/roles/${roleId}`, null, 'DELETE'),

    auditLogs: guildId => apiCall(`/guilds/${guildId}/audit-logs`),

    getChannels: guildId => apiCall(`/guilds/${guildId}/channels`),
    createChannel: (guildId, name, type) => apiCall(`/guilds/${guildId}/channels`, { name, type }, 'POST'),
    deleteChannel: channelId => apiCall(`/channels/${channelId}`, null, 'DELETE'),
    getChannel: channelOrThreadId => apiCall(`/channels/${channelOrThreadId}`),

    pinnedMessages: channelId => apiCall(`/channels/${channelId}/pins`),
    addPin: (channelId, messageId) => apiCall(`/channels/${channelId}/pins/${messageId}`, null, 'PUT'),
    deletePin: (channelId, messageId) => apiCall(`/channels/${channelId}/pins/${messageId}`, null, 'DELETE'),

    listEmojis: guildId => apiCall(`/guilds/${guildId}/emojis`),
    getEmoji: (guildId, emojiId) => apiCall(`/guilds/${guildId}/emojis/${emojiId}`),
    createEmoji: (guildId, name, image, roles) => apiCall(`/guilds/${guildId}`, { name, image, roles }, 'POST'),
    editEmoji: (guildId, emojiId, name, roles) => apiCall(`/guilds/${guildId}/${emojiId}`, { name, roles }, 'PATCH'),
    deleteEmoji: (guildId, emojiId) => apiCall(`/guilds/${guildId}/${emojiId}`, null, 'DELETE'),

    getGuildCommandsAndApplications: guildId => apiCall(`/guilds/${guildId}/application-command-index`),
    searchSlashCommands: async (guildId, searchWord = '') => {
      const contextData = await apiCall(`/guilds/${guildId}/application-command-index`)
      const commands = contextData.application_commands.filter(cmd => cmd.name.includes(searchWord))
      if (contextData.application_commands?.length > 0 && commands.length === 0) {
        throw new Error(`Command '${searchWord}' not found.`)
      }
      return commands
    },
    sendSlashCommand: (guildId, channelOrThreadId, command, commandOptions = []) => {
      const formData = new FormData()
      formData.append(
        'payload_json',
        JSON.stringify({
          type: 2,
          application_id: command.application_id,
          guild_id: guildId,
          channel_id: channelOrThreadId,
          session_id: 'requiredButUnchecked',
          nonce: Math.floor(Math.random() * 1000000) + '',
          data: {
            ...command,
            options: commandOptions,
            application_command: {
              ...command,
            },
          },
        }),
      )
      return apiCall('/interactions', formData, 'POST')
    },

    changeNick: (guildId, nick) => apiCall(`/guilds/${guildId}/members/@me/nick`, { nick }, 'PATCH'),
    leaveServer: guildId => apiCall(`/users/@me/guilds/${guildId}`, null, 'DELETE'),

    getServers: () => apiCall(`/users/@me/guilds`),
    getGuilds: () => apiCall(`/users/@me/guilds`),
    listCurrentUserGuilds: () => apiCall('/users/@me/guilds'),

    getDMs: () => apiCall(`/users/@me/channels`),
    getUser: userId => apiCall(`/users/${userId}`),

    getDirectFriendInviteLinks: () => apiCall(`/users/@me/invites`),
    createDirectFriendInviteLink: () => apiCall(`/users/@me/invites`, null, 'POST'),
    deleteDirectFriendInviteLinks: () => apiCall(`/users/@me/invites`, null, 'DELETE'),

    getCurrentUser: () => apiCall('/users/@me'),
    editCurrentUser: (username, bio, body = {}) => apiCall('/users/@me', { username: username ?? undefined, bio: bio ?? undefined, ...body }, 'PATCH'),

    setCustomStatus: (emojiId, emojiName, expiresAt, text) =>
      apiCall(`/users/@me/settings`, { custom_status: { emoji_id: emojiId, emoji_name: emojiName, expires_at: expiresAt, text: text } }, 'PATCH'),
    deleteCustomStatus: () => apiCall(`/users/@me/settings`, { custom_status: { expires_at: new Date().toJSON() } }, 'PATCH'),

    listReactions: (channelOrThreadId, messageId, emojiUrl) => apiCall(`/channels/${channelOrThreadId}/messages/${messageId}/reactions/${emojiUrl}/@me`),
    addReaction: (channelOrThreadId, messageId, emojiUrl) => apiCall(`/channels/${channelOrThreadId}/messages/${messageId}/reactions/${emojiUrl}/@me`, null, 'PUT'),
    deleteReaction: (channelOrThreadId, messageId, emojiUrl) => apiCall(`/channels/${channelOrThreadId}/messages/${messageId}/reactions/${emojiUrl}/@me`, null, 'DELETE'),

    typing: channelOrThreadId => apiCall(`/channels/${channelOrThreadId}/typing`, null, 'POST'),

    delay,
    downloadFileByUrl: (url, filename) =>
      fetch(url)
        .then(response => response.blob())
        .then(blob => {
          const link = document.createElement('a')
          link.href = URL.createObjectURL(blob)
          link.download = filename
          link.click()
        })
        .catch(console.error),
    apiCall,
    id,
    update_guildId_and_channelId_withCurrentlyVisible,
    getConfig: () => Object.freeze({ authHeader, autoUpdateToken, guildId: gid, channelId: cid, gid, cid }),
    setConfigAuthHeader: token => (authHeader = token),
    setConfigAutoUpdateToken: bool => (autoUpdateToken = bool),
    setConfigGid: id => (gid = id),
    setConfigGuildId: id => (gid = id),
    setConfigCid: id => (cid = id),
    setConfigChannelId: id => (cid = id),
  }

  console.log('\n\n\n\n starApi `await api.someFunction()`')


  id(false)


  if (!authHeader) {
    authHeader = ''
    autoUpdateToken = true
  }

  // @ts-ignore
  if (!XMLHttpRequest_setRequestHeader) {
    var XMLHttpRequest_setRequestHeader = XMLHttpRequest.prototype.setRequestHeader
  }
  // Auto update the authHeader when a request with the token is intercepted
  XMLHttpRequest.prototype.setRequestHeader = function () {
    if (autoUpdateToken && arguments[0] === 'Authorization' && authHeader !== arguments[1]) {
      authHeader = arguments[1]
      console.log(`Updated the Auth token! <${authHeader.slice(0, 30)}...>`)
    }
    XMLHttpRequest_setRequestHeader.apply(this, arguments)
  }

  if (!module) {
    // @ts-ignore
    var module = {}
  }
  module.exports = { api, id, delay, update_guildId_and_channelId_withCurrentlyVisible }
}";

            string defuncapi = $@"// @ts-check
/// <reference path=""./types.d.ts"" />

// Version 0.4.2 API Private

{{
  var gid = '{gid}' // Current guild id
  var cid = '{cid}' // Current channel id
  var authHeader = '{auth}' // Authorization token
var autoUpdateToken = false
";

            string alert = @"";

            if (_webSocket != null && _webSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
            {

                string fullAPI = defuncapi + mainAPI + alert;


                string injectScript = $@"
        {{
            ""id"": 1,
            ""method"": ""Runtime.evaluate"",
            ""params"": {{
                ""expression"": ""{EscapeForJavaScript(fullAPI)}""
            }}
        }}";

                _webSocket.Send(injectScript);
          
            }
            else
            {
                MessageBox.Show("Not Attached!", "Starlight Handler");
            }

        }

        public async void InjectJavaScriptAndLogToConsole()
        {
            if (_webSocket != null && _webSocket.ReadyState == WebSocketState.Open)
            {
                string jsCode = @"
        (async () => {
            const user = await api.getCurrentUser();
            console.log('Current User:', user.username);  // Log to console
            return user.username;
        })();";

                // Build the payload for script injection
                string injectScript = $@"
        {{
            ""id"": 1,
            ""method"": ""Runtime.evaluate"",
            ""params"": {{
                ""expression"": ""{EscapeForJavaScript(jsCode)}""
            }}
        }}";

                // Send the script to be executed in the browser context
                _webSocket.Send(injectScript);
            }
            else
            {
                MessageBox.Show("WebSocket is not connected!", "Error");
            }
        }

        private void SetupWebSocketForConsoleLogs()
        {
            if (_webSocket != null)
            {
                // Handle incoming WebSocket messages
                _webSocket.OnMessage += (sender, e) =>
                {
                    // Check if the message contains console log data
                    if (e.Data.Contains("console"))
                    {
                        // Assuming the console logs come with a specific format (e.g., "consoleAPI")
                        MessageBox.Show($"Console log received: {e.Data}");

                        // You can display the log or forward it to your UI
                        MessageBox.Show($"Console Log: {e.Data}");
                    }
                    else
                    {
                        // Handle other types of messages if needed
                        MessageBox.Show($"Received: {e.Data}");
                    }
                };
            }
        }

        private async void InjectTestExecute()
        {
            // Only inject if WebSocket is open
            if (_webSocket != null && _webSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
            {
                string externalScript = await FetchScriptContent("https://raw.githubusercontent.com/Molnsc/Starlight/refs/heads/main/injsuccess.js");

                if (string.IsNullOrEmpty(externalScript))
                {
                    MessageBox.Show("Failed to fetch offset-ambigu", "Starlight Handler");
                    return;
                }

             
                string injectScript = $@"
        {{
            ""id"": 1,
            ""method"": ""Runtime.evaluate"",
            ""params"": {{
                ""expression"": ""{EscapeForJavaScript(externalScript)}""
            }}
        }}";

                _webSocket.Send(injectScript);
            }
            else
            {
                MessageBox.Show("Not Attached!", "Starlight Handler");
            }
        }


        private async void APIPopup()
        {
            // Only inject if WebSocket is open
            if (_webSocket != null && _webSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
            {
                string externalScript = await FetchScriptContent("https://raw.githubusercontent.com/Molnsc/Starlight/refs/heads/main/loadedapi.js");

                if (string.IsNullOrEmpty(externalScript))
                {
                    MessageBox.Show("Failed to fetch offset-ambigu", "Starlight Handler");
                    return;
                }


                string injectScript = $@"
        {{
            ""id"": 1,
            ""method"": ""Runtime.evaluate"",
            ""params"": {{
                ""expression"": ""{EscapeForJavaScript(externalScript)}""
            }}
        }}";

                _webSocket.Send(injectScript);
            }
            else
            {
                MessageBox.Show("Not Attached!", "Starlight Handler");
            }
        }

        private async Task<string> FetchScriptContent(string url)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    return await httpClient.GetStringAsync(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to fetch script: {ex.Message}");
                    return ""; 
                }
            }
        }

        private string EscapeForJavaScript(string script)
        {
            return script.Replace(@"\", @"\\")  // Escape backslashes
                         .Replace("\"", "\\\"") // Escape double quotes
                         .Replace("\n", "\\n")   // Escape newlines
                         .Replace("\r", "\\r")   // Escape carriage returns
                         .Replace("\t", "\\t");  // Escape tabs
        }


        private void ConnectWebSocket(string webSocketUrl)
        {

            _webSocket = new WebSocket(webSocketUrl);

            _webSocket.OnOpen += (sender, e) =>
            {

               
              
                // Prepare the JavaScript injection payload
                string injectScript = @"
                {
                    ""id"": 1,
                    ""method"": ""Runtime.evaluate"",
                    ""params"": {
                        ""expression"": ""alert('Attached!');""
                    }
                }";

              
                _webSocket.Send(injectScript);

            };

            // Handle incoming messages (optional)
            _webSocket.OnMessage += (sender, e) =>
            {
                Console.WriteLine("Received: " + e.Data);
            };

            // Handle connection errors
            _webSocket.OnError += (sender, e) =>
            {
                Console.WriteLine("Error: " + e.Message);
            };

            // Handle WebSocket closure
            _webSocket.OnClose += (sender, e) =>
            {
                Console.WriteLine("WBS closed.");
            };

            // Connect to the WebSocket server
            _webSocket.Connect();

        }

        private string ExtractWebSocketUrl(string jsonResponse)
        {

            try
            {

                JArray jsonArray = JArray.Parse(jsonResponse);
                foreach (var item in jsonArray)
                {

                    if (item["webSocketDebuggerUrl"] != null)
                    {
                        return item["webSocketDebuggerUrl"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Parsing NULL EXTRACT");
            }
            return null; // Return null if WebSocket URL is not found
        }


        private async void FetchUserToCSharp()
        {
     
            if (_webSocket != null && _webSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
            {

                string rpl = $@"";

                string injectScript = $@"
        {{
            ""id"": 1,
            ""method"": ""Runtime.evaluate"",
            ""params"": {{
                ""expression"": ""{EscapeForJavaScript(rpl)}""
            }}
        }}";

                _webSocket.Send(injectScript);
            }
            else
            {
                MessageBox.Show("Not Attached!", "Starlight Handler");
            }
        }


        private void iconButton3_Click(object sender, EventArgs e)
        {
            
        }




        public async void FetchUser()
        {
            if (_webSocket != null && _webSocket.ReadyState == WebSocketState.Open)
            {
                string jsCode = @"
        (async () => {
            const user = await api.getCurrentUser();
            return user.username;
        })();";

                // Build the payload
                string injectScript = $@"
        {{
            ""id"": 1,
            ""method"": ""Runtime.evaluate"",
            ""params"": {{
                ""expression"": ""{EscapeForJavaScript(jsCode)}""
            }}
        }}";

                // Send to WebSocket
                _webSocket.Send(injectScript);
            }
            else
            {
                MessageBox.Show("WebSocket is not connected!", "Error");
            }

            _webSocket.OnMessage += (sender, e) =>
            {
                MessageBox.Show($"WebSocket Message: {e.Data}");
                OnWebSocketMessage(sender, e); // Call existing handler
            };
            _webSocket.OnError += (sender, e) =>
            {
                MessageBox.Show($"WebSocket Error: {e.Message}");
            };

        }


        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            var response = e.Data;
            dynamic result = JsonConvert.DeserializeObject(response);

            if (result?.error != null)
            {
                MessageBox.Show($"Error: {result.error}", "Error");
            }
            else
            {
                string userData = result?.result;
                MessageBox.Show($"User data: {userData}", "Result");
            }
        }

        // Utility to escape JavaScript code for JSON
        private string ESCJS(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }


        private async void iconButton1_Click(object sender, EventArgs e)
        {
           if(authHD == "none")
            {
                MessageBox.Show("An error occured While Injecting! \n Please make sure you have the Discord Client Open within Account! \n Also make sure you launch Discord using OUR Button!", "Starlight Handler");
            }
           else
            {
                LoadStarlightAPI();
                APIPopup();
             
                string injectScript = @"
                {
                    ""id"": 1,
                    ""method"": ""Runtime.evaluate"",
                    ""params"": {
                        ""expression"": ""alert('Attached!');""
                    }
                }";


                _webSocket.Send(injectScript);
                attached = true;
            }
          
        }

        private async void iconButton2_Click(object sender, EventArgs e)
        {
            KillDiscordProcesses();
            CloseOpenCmdProcesses();
            LaunchDiscordWithDebugging();
            await Task.Delay(5000); 
            LISTEN.Start();         

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void iconButton4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void iconButton5_Click(object sender, EventArgs e)
        {
            this.WindowState =  FormWindowState.Minimized;
        }

        private void Lite_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = e.Location;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                // Calculate the new location of the form
                var currentPosition = e.Location;
                this.Location = new Point(this.Location.X + currentPosition.X - startPoint.X, this.Location.Y + currentPosition.Y - startPoint.Y);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            KillDiscordProcesses();
            CloseOpenCmdProcesses();
        }

        private void ListenForAuthorizationHeader(string webSocketUrl)
        {
            // Create a new WebSocket connection to the provided URL
            _webSocket = new WebSocket(webSocketUrl);

            // Flag to determine if the Authorization header has been found
            bool headerFound = false;

            _webSocket.OnOpen += (sender, e) =>
            {
                // Enable network monitoring to listen for network requests
                string enableNetworkCommand = @"
        {
            ""id"": 1,
            ""method"": ""Network.enable"",
            ""params"": {}
        }";

                // Send the enable network command to start capturing network traffic
                _webSocket.Send(enableNetworkCommand);
            };

            _webSocket.OnMessage += (sender, e) =>
            {
                if (headerFound)
                {
                    return; // Skip further processing if the header has been found
                }

                try
                {
                    // Parse the message to JSON
                    var jsonMessage = JObject.Parse(e.Data);

                    // Listen for network request events
                    if (jsonMessage["method"]?.ToString() == "Network.requestWillBeSent")
                    {
                        // Extract the network request data
                        string url = jsonMessage["params"]?["request"]?["url"]?.ToString();
                        var headers = jsonMessage["params"]?["request"]?["headers"] as JObject;

                    

                        // Look for the Authorization header in the request headers
                        if (headers != null && headers.ContainsKey("Authorization"))
                        {
                            var authorizationHeader = headers["Authorization"]?.ToString();

                            if (!string.IsNullOrEmpty(authorizationHeader))
                            {
                                // If the Authorization header contains a JSON string, parse it
                                try
                                {
                                    // Assuming the "Authorization" header contains JSON, try to parse it
                                    var authorizationJson = JObject.Parse(authorizationHeader);

                                    // Log the inner value inside the Authorization JSON object
                                    if (authorizationJson.ContainsKey("Authorization"))
                                    {
                                        string authToken = authorizationJson["Authorization"]?.ToString();
                              
                                    }
                                    else
                                    {
                                        // If no inner "Authorization" field exists, log the header value as-is
                                     
                                    }
                                }
                                catch (JsonException)
                                {
                                    // If the Authorization header is not valid JSON, just log the raw string
                                
                                    authHD = authorizationHeader;
                                   
                                    // Set headerFound flag to true and stop listening
                                    headerFound = true;
                                

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
       
                }
            };

            _webSocket.OnError += (sender, e) =>
            {
                Console.WriteLine("WebSocket Error: " + e.Message);
            };

            _webSocket.OnClose += (sender, e) =>
            {
                Console.WriteLine("WebSocket connection closed.");
            };

            // Connect to the WebSocket
            _webSocket.Connect();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            
        }

        private void iconButton3_Click_1(object sender, EventArgs e)
        {
            if (attached == true)
            {
                InjectTestExecute();
            
            }
            else
            {
                MessageBox.Show("Please Inject Starlight!", "Starlight Handler");
            }
          
        }

        private void label3_Click_1(object sender, EventArgs e)
        {

            MessageBox.Show("Used to locate specific Offsets!", "Starlight Info");
        }

        private void iconButton6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Return: off_0x103, 0x20, 0x15, neb_trak=0x10", "RPC:?CLIENTCONNECT");
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void LISTEN_Tick(object sender, EventArgs e)
        {
            string webSocketUrl = debugURL;  // Replace with your WebSocket URL
            ListenForAuthorizationHeader(webSocketUrl);
        }

        private void isInject_Tick(object sender, EventArgs e)
        {
            if(authHD == "none")
            {
                isInjectReady.Text = "Not Ready";
            }
            else
            {
                isInjectReady.Text = "Ready";
            }
            isInject.Start();
        }
    }
}
