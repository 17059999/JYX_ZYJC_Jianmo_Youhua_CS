# Synchronization Scripts

Please refer to [onenote](https://microsoft.sharepoint.com/teams/winteropsh/_layouts/OneNote.aspx?id=%2Fteams%2Fwinteropsh%2FShared%20Documents%2FOpen%20Source&wd=target%28Contribute%20to%20test%20suite.one%7CE8E61518-B611-4B3D-8FE5-106A611A4C31%2FSynchronization%20Scripts%7CBCBF01DC-4ACC-4E08-8443-8830A40AFD28%2F%29)

# Prepare the sync machine

The scripts are using ssh to connect to git repo.

1. Install Git for Windows.
2. Open git bash and run `ssh-keygen` to generate an SSH key pair.
3. Add the public key on VSO: https://iatsh.visualstudio.com/_details/security/keys
4. Add the public key on GitHub: https://github.com/settings/keys
5. Set ssh not to check server ip (because GitHub has many IPs, we don't want to save server fingerprint for each IP). Edit your ~/.ssh/config as:
```
Host *
  CheckHostIP no
```
6. Save VSO's fingerprint: `ssh-keyscan iatsh.visualstudio.com >> ~/.ssh/known_hosts`
7. Save GitHub's fingerprint: `ssh-keyscan github.com >> ~/.ssh/known_hosts`
