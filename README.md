# CryptLink.Host
This is a very basic implementation of a storage server using existing CryptLink packages

In it's current state 

## Config
You may config the kestrel endpoints in appsettings.json

``` json
"Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://localhost:5500"
      },

      "HttpsInlineCertFile": {
        "Url": "https://localhost:5501",
        "Certificate": {
          "Path": "<path to .pfx file>",
          "Password": "<certificate password>"
        }
      },

      "HttpsInlineCertStore": {
        "Url": "https://localhost:5502",
        "Certificate": {
          "Subject": "<subject; required>",
          "Store": "<certificate store; defaults to My>",
          "Location": "<location; defaults to CurrentUser>",
          "AllowInvalid": "<true or false; defaults to false>"
        }
      },

      "HttpsDefaultCert": {
        "Url": "https://localhost:5503"
      },

      "Https": {
        "Url": "https://*:5504",
        "Certificate": {
          "Path": "<path to .pfx file>",
          "Password": "<certificate password>"
        }
      }
    },
    "Certificates": {
      "Default": {
        "Path": "<path to .pfx file>",
        "Password": "<certificate password>"
      }
    }
}
```

Or by command line: 

`dotnet run --server.urls "http://localhost:5100;http://localhost:5101;http://*:5102"`

Or the environment variable: `CL_ASPNETCORE_URLS`

### Limits
https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits?view=aspnetcore-2.1

# CryptLink.Host.Deploy
A simple tool to seed an empy host with HTML files
