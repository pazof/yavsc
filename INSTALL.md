# configuring a new application

## Resources

### A Name

In this doc, let's say, `FreeField`

### A domain name

Wait ... `pschneider.fr`
You'll have to see with your provider, in order to power it, and make it point to your host ip, at least concerning the ports 80 & 443.

### An Npgsql db

The database must be created, and we need its connection string, allowed to modify the dd

### A mailling service

You'll need to send e-mail ... using smtp and the provider of your choice.

### Google

I'll have to make with it, a least a moment, it needs a *service account*, and some setup, 
in the `appsettings.*.json` file, and 
the execution environment variable GOOGLE_APPLICATION_CREDENTIALS to a value pointing the json description file for the Google Service account.


