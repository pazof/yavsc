# Install

With internet access, Git, dotnet, and GNU/Makefile, simply follow these instructions:

```bash
git clone https://github.com/pschneider/yavsc.git
cd yavsc
dotnet restore
cd contrib
make install
```

## Resources

System resources needed to install.

### Some TCP/IP ports to listen

The following provided services need all a port to listen :

* Org
* Blogs
* Api (to become obsolete, in favor of Performer + Client + Com + Moderation )

### A Software bloc name

In this doc, let's say, `Yavsc`

### A domain name

Wait ... `pschneider.fr`
You'll have to see with your provider, in order to power it, and make it point to your host ip, at least concerning the ports 80 & 443.

### A Postgresql db

The database must be created, and we need its connection string, allowed to modify the data dictionary, see the `.env` file

### An Smtp mail service

You'll need to send e-mail ... using smtp and the provider of your choice, see the `.env` file.

### Google

I'll have to make with it, a least a moment, it needs a *service account*, and some setup, 
in the `appsettings.*.json` file, and 
the execution environment variable GOOGLE_APPLICATION_CREDENTIALS to a value pointing the json description file for the Google Service account.
