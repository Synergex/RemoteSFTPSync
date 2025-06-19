[![Build and Package .NET 8 Solution](https://github.com/Synergex/RemoteSFTPSync/actions/workflows/build.yml/badge.svg)](https://github.com/Synergex/RemoteSFTPSync/actions/workflows/build.yml)

# SFTP Sync

SFTP Sync is a utility application that can synchronize a local directory
structure and files to a remote OpenVMS system via the Secure FTP protocol.

When first activated the product will checks whether the local and
remote directories are synchronized, and if not will update the remote
directories and files to match the local directories and files.

Once synchronized the application then starts monitoring the local file
system for changes and replicates those changes in the remote system in
near-real-time.

Two versions of the application are available, a Windows desktop application,
and a command line application.

For additional information on how to use the application, please refer to the
Windows help file once installed.
