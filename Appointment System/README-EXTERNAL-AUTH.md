# External Authentication Setup for SPA

This application supports authentication via Google and GitHub with a Single Page Application (SPA) frontend.

## Google Authentication

1. Create a project in the [Google Cloud Console](https://console.cloud.google.com/)
2. Navigate to "APIs & Services" > "Credentials"
3. Click "Create Credentials" > "OAuth client ID"
4. Select "Web application" as the application type
5. Set a name for your client
6. Add authorized JavaScript origins:
   - For development: `http://localhost:3000` (or whatever port your SPA runs on)
   - For production: `https://your-domain.com`
7. Add authorized redirect URIs:
   - For development: `http://localhost:3000` (or whatever port your SPA runs on)
   - For production: `https://your-domain.com`
8. Click "Create"
9. Note your Client ID and Client Secret

### Configure in your application:

For development, use user secrets:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
```

For production, update the values in your hosting environment variables or deployment settings.

## GitHub Authentication

1. Go to your GitHub account settings
2. Navigate to "Developer settings" > "OAuth Apps"
3. Click "New OAuth App"
4. Fill out the registration form:
   - Application name: Your app name
   - Homepage URL: Your app's homepage URL
   - Authorization callback URL:
     - For development: `http://localhost:3000` (or whatever port your SPA runs on)
     - For production: `https://your-domain.com`
5. Click "Register application"
6. Note your Client ID
7. Generate a new Client Secret and note it down

### Configure in your application:

For development, use user secrets:

```bash
dotnet user-secrets set "Authentication:GitHub:ClientId" "your-github-client-id"
dotnet user-secrets set "Authentication:GitHub:ClientSecret" "your-github-client-secret"
```

For production, update the values in your hosting environment variables or deployment settings.

## Frontend Implementation

In your SPA frontend, you'll need to implement provider-specific authentication.

### Example with Google (Frontend):

```javascript
// Using Google OAuth client library
function initializeGoogleAuth() {
  gapi.load('auth2', function() {
    gapi.auth2.init({
      client_id: 'YOUR_GOOGLE_CLIENT_ID'
    });
  });
}

async function signInWithGoogle() {
  const googleAuth = gapi.auth2.getAuthInstance();
  const googleUser = await googleAuth.signIn();
  const idToken = googleUser.getAuthResponse().id_token;
  
  // Send token to your backend
  const response = await fetch('/account/signin-google', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      token: idToken,
      rememberMe: true
    })
  });
  
  const data = await response.json();
  // Store the JWT token from the response
  localStorage.setItem('token', data.token);
}
```

### Example with GitHub (Frontend):

```javascript
// Using OAuth2 Implicit Flow
function signInWithGitHub() {
  const clientId = 'YOUR_GITHUB_CLIENT_ID';
  const redirectUri = encodeURIComponent(window.location.origin);
  
  window.location.href = `https://github.com/login/oauth/authorize?client_id=${clientId}&redirect_uri=${redirectUri}&scope=user:email`;
}

// Handle the callback (in your callback handler component)
async function handleGitHubCallback() {
  // Get the code from URL
  const code = new URLSearchParams(window.location.search).get('code');
  
  // Exchange code for token using our backend endpoint
  const response = await fetch('/account/github-token-exchange', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ 
      code,
      redirectUri: window.location.origin 
    })
  });
  
  const { access_token } = await response.json();
  
  // Now send the token to your backend
  const loginResponse = await fetch('/account/signin-github', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      token: access_token,
      rememberMe: true
    })
  });
  
  const data = await loginResponse.json();
  // Store the JWT token from the response
  localStorage.setItem('token', data.token);
}
```

## Backend API Endpoints

The API provides two separate endpoints for authentication:

1. Google Sign-In:
   - Endpoint: `/account/signin-google`
   - Method: POST
   - Payload:
     ```json
     {
       "token": "The ID token from Google",
       "rememberMe": true/false
     }
     ```

2. GitHub Sign-In:
   - Endpoint: `/account/signin-github`
   - Method: POST
   - Payload:
     ```json
     {
       "token": "The access token from GitHub",
       "rememberMe": true/false
     }
     ```

3. GitHub Token Exchange (needed to convert the authorization code to a token):
   - Endpoint: `/account/github-token-exchange`
   - Method: POST
   - Payload:
     ```json
     {
       "code": "The authorization code from GitHub",
       "redirectUri": "The redirect URI used in the authorization request"
     }
     ```

All authentication endpoints return a JWT token that should be used for subsequent API calls.

## Security Notes

1. In a production environment, always use HTTPS for all communications.
2. Validate tokens on the server side to prevent forgery.
3. For GitHub, you need the additional token exchange step due to OAuth flow requirements.
4. Never expose your client secrets in frontend code. 