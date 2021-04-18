# Azure Communication Service and MS Teams interoperability demo
 
Azure Communication Services(ACS) allows you to develop real-time communication features for your application. Within upcoming releases Microsoft provides interoperability with Teams. Currently ACS and Microsoft Teams interoperability is in "Preview".

Within this simple repository, I tried to understand upcoming features with this interoperability. Basically with this simple ASP.NET Core Web Application(w/React), a user can attend to a MS Teams meeting with voice and video call.

Looking forward for "GA" and try to share as I dig more about it.

## UPDATES:
- [18-04-2021] Graph API integration to get MS Teams' user presence 
- [11-04-2021] Text based chat with MS Teams client is added
- [11-04-2021] Some UI improvements

References:
- [Teams interoperability](https://docs.microsoft.com/en-us/azure/communication-services/concepts/teams-interop)
- [Get Started](https://docs.microsoft.com/en-us/azure/communication-services/quickstarts/voice-video-calling/get-started-teams-interop?pivots=platform-web)
- [MS Teams User presence](https://docs.microsoft.com/en-us/graph/api/presence-get?view=graph-rest-1.0&tabs=http)
- [Microsoft Graph](https://docs.microsoft.com/en-us/graph/overview)
- [Microsoft Graph Authentication Providers](https://docs.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS)

| Away  | Online |
| ------------- | ------------- |
| <img src="https://user-images.githubusercontent.com/4550197/115149333-3b7ea600-a06c-11eb-8c55-959fc1b98b60.png" height="250">  |<img src="https://user-images.githubusercontent.com/4550197/115149334-3f122d00-a06c-11eb-90aa-9da0410364bc.png" height="250"> |



![chat-with-teams](https://user-images.githubusercontent.com/4550197/114297048-2081c400-9ab7-11eb-818b-0c97c3313021.png)
![join-meeting](https://user-images.githubusercontent.com/4550197/114276534-aeb26780-9a2f-11eb-9630-574b8b6adf3a.png)
![meeting-with-camera](https://user-images.githubusercontent.com/4550197/114276535-afe39480-9a2f-11eb-9c1e-ac74b3ce8cb4.png)
