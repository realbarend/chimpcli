# TimeChimp CLI

This tool is an unsupported command line interface for TimeChimp time tracking.

## Disclaimer

Use this software at own risk: it can potentially corrupt or remove your timechimp data or maybe mess up the whole organisation.
Don't come to me when things get ugly. Otoh I use the tool myself every day and I think it is safe.

This software is not in any way supported by TimeChimp. It was also not created by the TimeChimp developers.

The api calls used to communicate with TimeChimp are not documented as far as I know.
If you find public documentation then please let me know!

## How to use

Easiest way is to just grab the executable for your OS from the most recent [Github Release](https://github.com/realbarend/chimpcli/releases) and put it somewhere in your $PATH.
If you use bash or similar shell, you can instead create an alias to the executable:

```bash
# add this to your .bashrc
alias c='chimp'
alias chimp='/path/to/chimp'
# now you can use `c` anywhere
```

Note: your antivirus may freak out when downloading or running unsigned tools like this.
I use BitDefender and it really does not like downloading or running unknown executables, for obvious reasons.
Feel free to review the code to make sure the code is clean.
The released executables are automatically generated from source using GitHub Actions, you can check that yourself as well. 

Note: don't be scared when the app throws dotnet exceptions at you.
These exceptions usually just try to tell you the app did not like your input.

Note: I usually use the tool on Windows 11 using git bash.
So you may run into build- or runtime issues when using a different environment. Let me know if you do.

You first need to log in. The cli asks for your timechimp credentials.
These will be only used to fetch an authtoken from timechimp.

Note: if you set environment variable `CHIMPCLI_PERSIST_LOGIN_CREDENTIALS=true`,
then your credentials will be remembered so they can be reused when logging in again at later time.

```
# chimp login
Enter your timechimp username: my.username@company.com
Enter your timechimp password: sdkfjskdfj
Login successful, got authtoken valid until <some future date>
```
Instead of logging in interactively,
you can also configure environment variables with your credentials:
```
#  export CHIMPCLI_USERNAME=my.username@company.com
#  export CHIMPCLI_PASSWORD=sdkfjkdjf
# chimp login
Reading your timechimp username from env.CHIMPCLI_USERNAME
Reading your timechimp password from env.CHIMPCLI_PASSWORD
Login successful, got authtoken valid until <some future date>
```

Then you need to know for which projects use can track hours.
Take note of the line numbers ('1', '2', ...).

```
# chimp projects

[1]  Customer - Project name
[2]  Customer - A second project
[3]  etc...

```

That's it, you can now add, update, remove or move tracked hours:

```
# chimp add p2 830-930 my first cli tracked hour hooray

MAANDAG =============================================================================
[ 1] A second project (Customer) p2                08:30-09:30 (1'00) my first cli tracked hour hooray


# chimp add p1 930+30 something else

MAANDAG =============================================================================
[ 1] A second project (Customer) p2                08:30-09:30 (1'00) my first cli tracked hour hooray
[ 2] Project name (Customer) p1                    09:30-10:00 (0'30) something else


# chimp update 1 updated my note

MAANDAG =============================================================================
[ 1] A second project (Customer) p2                08:30-09:30 (1'00) updated my note
[ 2] Project name (Customer) p1                    09:30-10:00 (0'30) something else

# chimp help

<this will list all currently supported commands>
```

## Language support

After logging in, the cli will by default use the language that you have configured in your TimeChimp profile.
However, cli translations are currently limited to Dutch and English.
You can override the language using the `CHIMPCLI_LANGUAGE` environment variable:

```bash
export CHIMPCLI_LANGUAGE=en
chimp
```

## Persisting state

The state is written to disk, to file ~/.chimpcli.
In this file, your auth token and your username are stored, along with some less interesting data (see [Models/StateData.cs](Models/StateData.cs)).
Optionally, also your login credentials are stored for easy re-authorization.
If on Windows, DPAPI is used to encrypt the data. On Linux or MacOs, the state file is protected with chmod 0600.

## How to build from source

You need [dotnet sdk 8](https://dotnet.microsoft.com/en-us/download) to be able to compile the project.

```bash
# run from source
dotnet run --project Chimp

# or: build executable and run that
dotnet publish -c Release -o out
./out/chimp

# you can also build a self-contained executable,
# that can run without having dotnet installed
dotnet publish -c Release -o out --self-contained
```

OEF
