
---

## Very important before upload
This project contains sensitive config values in `Web.config`.

You should remove or replace:
- SQL connection string
- DB username/password
- Twilio SID
- Twilio token
- login username/password
- any email password

### Replace them like this
In `Web.config`, change to safe placeholders:

```xml
<add name="TwilioConnection" connectionString="YOUR_CONNECTION_STRING" providerName="System.Data.SqlClient" />
<add key="SID" value="YOUR_TWILIO_SID" />
<add key="Token" value="YOUR_TWILIO_AUTH_TOKEN" />
<add key="Username" value="YOUR_PORTAL_USERNAME" />
<add key="Password" value="YOUR_PORTAL_PASSWORD" />
<add key="TestSID" value="YOUR_TEST_TWILIO_SID" />
<add key="TestToken" value="YOUR_TEST_TWILIO_AUTH_TOKEN" />
