# Security policy

## Reporting a vulnerability

Please **do not** open a public issue for security problems.

Send a private report to **ahmed.elgamal.dev22@gmail.com** with:

- a short description of the issue,
- steps to reproduce,
- affected version/commit,
- any suggested mitigation.

You can expect an acknowledgement within a few days. Please allow reasonable time for a fix before public disclosure.

## Supported versions

This is a personal portfolio project maintained on the `main` branch. Security fixes are applied there.

## Deployment checklist

- [ ] Set `SeedAdmin` credentials through environment variables or user secrets.
- [ ] Set a strong admin password and rotate it if it was ever exposed.
- [ ] Use a least-privilege SQL Server login.
- [ ] Enable HTTPS and keep HSTS enabled outside Development.
- [ ] Restrict write access to `wwwroot/images` and `wwwroot/files`.
- [ ] Back up the database and uploads directory.
- [ ] Review the Content-Security-Policy if you add new third-party scripts.
