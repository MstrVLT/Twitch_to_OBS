# Twitch_to_OBS

Go to https://dev.twitch.tv/console/apps/create

![image](https://user-images.githubusercontent.com/10544388/213574205-dde4c41f-a221-41da-bee4-f111f9e33e86.png)

Go to 
```
id.twitch.tv/oauth2/authorize?response_type=token&client_id=
your_client_id
&redirect_uri=http://localhost&scope=channel:read:redemptions&state=67ab8aa609ea11e793ae9236143431
```

Redirect to 
```
localhost/#access_token=
your_access_token
&scope=channel%3Aread%3Aredemptions&state=67ab8aa609ea11e793ae9236143431&token_type=bearer
```

Edit appsettings.json and run exe
