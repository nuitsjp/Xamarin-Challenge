using CoolBreeze.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoolBreeze
{
    public class SubmissionInformation : Common.ObservableBase
    {
        private string _displayName;
        public string DisplayName
        {
            get { return this._displayName; }
            set { this.SetProperty(ref this._displayName, value); }
        }

        private string _emailAddress;
        public string EmailAddress
        {
            get { return this._emailAddress; }
            set { this.SetProperty(ref this._emailAddress, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return this._isBusy; }
            set { this.SetProperty(ref this._isBusy, value); }
        }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get { return this._isAuthenticated; }
            set { this.SetProperty(ref this._isAuthenticated, value); }
        }

        private bool _isSubmitted;
        public bool IsSubmitted
        {
            get { return this._isSubmitted; }
            set { this.SetProperty(ref this._isSubmitted, value); }
        }

        public string SubmissionIcon
        {
            get { return "key"; }
        }

        private string _authenticateLabel;
        public string AuthenticateLabel
        {
            get { return this._authenticateLabel; }
            set { this.SetProperty(ref this._authenticateLabel, value); }
        }

        private string _submitLabel;
        public string SubmitLabel
        {
            get { return this._submitLabel; }
            set { this.SetProperty(ref this._submitLabel, value); }
        }

        private GraphUserInformation _currentUser;
        public GraphUserInformation CurrentUser
        {
            get { return this._currentUser; }
            set { this.SetProperty(ref this._currentUser, value); }
        }

        public void Update(GraphUserInformation profile)
        {
            this.CurrentUser = profile;

            this.DisplayName = profile.displayName;
            this.EmailAddress = profile.userPrincipalName;
            this.SubmitLabel = "Submit my solution";
        }

        public System.Windows.Input.ICommand AuthorizeUserCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    this.IsBusy = true;

                    var profile = await Helpers.SubmissionHelper.ValidateAsync();

                    this.IsAuthenticated = profile != null;

                    if (this.IsAuthenticated) this.Update(profile);

                    this.IsBusy = false;

                }, CanAuthorize);
            }
        }

        public System.Windows.Input.ICommand SubmitChallengeCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    this.IsBusy = true;
                    
                    await Helpers.SubmissionHelper.SubmitAsync(this.CurrentUser);
                    
                    this.IsSubmitted = true;
                    this.IsBusy = false;

                }, CanSubmit);
            }
        }

        private bool CanAuthorize() => !_isAuthenticated;
        private bool CanSubmit() => _isAuthenticated;

    }
    
    public class GraphUserInformation
    {
        public string givenName { get; set; }
        public string surname { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public string userPrincipalName { get; set; }
        public object[] businessPhones { get; set; }
        public object jobTitle { get; set; }
        public object mail { get; set; }
        public object mobilePhone { get; set; }
        public object officeLocation { get; set; }
        public object preferredLanguage { get; set; }
    }
}

namespace CoolBreeze.Helpers
{
    public static class SubmissionHelper
    { 
        public static async Task<GraphUserInformation> ValidateAsync()
        {
            GraphUserInformation authenticatedUser = null;

            string[] scopes = { "User.Read", "User.ReadBasic.All" };

            try
            {
                var result = await App.AuthenticationClient.AcquireTokenAsync(scopes);

                authenticatedUser = await GetUserProfileAsync(result.Token);

            }
            catch (Exception ex)
            {

            }

           
            
            return authenticatedUser;
        }

        public static async Task<bool> SubmitAsync(GraphUserInformation user)
        {
            HttpClient client = new HttpClient();
            
            try
            {
                string url = $"https://traininglabservices.azurewebsites.net/api/submissions/?id={user.id}&displayName={user.displayName}&emailAddress={user.userPrincipalName}&installId={Microsoft.Azure.Mobile.MobileCenter.InstallId}";


                var result = await client.PostAsync(url, null);

            }
            catch (Exception ex)
            {

            }
                       
            return true;
        }

        private static async Task<GraphUserInformation> GetUserProfileAsync(string token)
        {
            GraphUserInformation user = new GraphUserInformation();

            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");

            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

            HttpResponseMessage response = await client.SendAsync(message);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<GraphUserInformation>(result);
            }
            else
            {

            }

            return user;
        }
    }

   

  
}
