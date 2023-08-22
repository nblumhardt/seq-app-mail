﻿using System;
using MailKit.Security;

namespace Seq.App.Mail.Smtp;

class SmtpOptions
{
    public string Host { get; }
    public int Port { get; }
    public string? Username { get; }
    public string? Password { get; }
    public SecureSocketOptions SocketOptions { get; }

    public bool RequiresAuthentication => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    public SmtpOptions(string host, int port, SecureSocketOptions socketOptions, string? username, string? password)
    {
        Host = host ?? throw new ArgumentNullException(nameof(host));
        Port = port;
        Username = username;
        Password = password;
        SocketOptions = socketOptions;
    }
}
