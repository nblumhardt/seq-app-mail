﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using Seq.Apps;
using Seq.Mail.TimeZones;
using Serilog.Events;

// ReSharper disable UnusedAutoPropertyAccessor.Global, MemberCanBePrivate.Global

namespace Seq.Mail
{
    public abstract class MailApp : SeqApp, ISubscribeToAsync<LogEvent>
    {
        const string DefaultSubjectTemplate = "{@Message}";

        MailMessageFactory? _mailMessageFactory;

        [SeqAppSetting(
            DisplayName = "From address",
            HelpText = "The address from which the email is being sent.")]
        public string? From { get; set; }

        [SeqAppSetting(
            DisplayName = "To address",
            Syntax = "template",
            IsInvocationParameter = true,
            HelpText = "The address to send email to. Multiple addresses are separated by " +
                       "commas or semicolons. Template syntax is supported.")]
        public string? To { get; set; }
        
        [SeqAppSetting(
            IsOptional = true,
            IsInvocationParameter = true,
            Syntax = "template",
            HelpText = "The message subject. Template syntax is supported.")]
        public string? Subject { get; set; }

        [SeqAppSetting(
            InputType = SettingInputType.LongText,
            IsOptional = true,
            Syntax = "template",
            HelpText = "The request body to send. Template syntax is supported.")]
        public string? Body { get; set; }
        
        [SeqAppSetting(
            InputType = SettingInputType.Checkbox,
            IsOptional = true,
            Syntax = "template",
            DisplayName = "Body is plain text",
            HelpText = "Treat the request body (above) as plain text. The default is to assume " +
                       "that the body is HTML and to escape string content appropriately.")]
        public bool BodyIsPlainText { get; set; }
        
        [SeqAppSetting(
            DisplayName = "Time zone name",
            IsOptional = true,
            HelpText = "The IANA name of the default time zone to use when formatting dates and " +
                       "times. The default is `Etc/UTC`.")]
        public string? TimeZoneName { get; set; }
        
        [SeqAppSetting(
            DisplayName = "Date/time format",
            IsOptional = true,
            HelpText = "A format string controlling how dates and times are formatted. Supports .NET date/time formatting " +
                       "syntax. The default is `o`, producing ISO-8601.")]
        public string? DateTimeFormat { get; set; }
        
        protected override void OnAttached()
        {
            _mailMessageFactory = new MailMessageFactory(
                MailboxAddress.Parse(From ?? throw new ArgumentException("A `From` address must be supplied.")),
                (To ?? throw new ArgumentException("At least one `To` address must be supplied."))
                    .Split(',', ';'),
                Subject ?? DefaultSubjectTemplate,
                Body ?? LoadDefaultBodyTemplate(),
                BodyIsPlainText,
                TimeZoneName ?? PortableTimeZoneInfo.UtcTimeZoneName,
                DateTimeFormat ?? "o",
                this.App,
                this.Host);
        }

        protected abstract Task SendAsync(MimeMessage message, CancellationToken cancel);

        public async Task OnAsync(Event<LogEvent> evt)
        {
            using var message = _mailMessageFactory!.FromEvent(evt.Data);
            await SendAsync(message, default);
        }
        
        internal static string LoadDefaultBodyTemplate()
        {
            var resourceStream = typeof(MailApp).Assembly.GetManifestResourceStream("DefaultBodyTemplate")!;
            return new StreamReader(resourceStream, System.Text.Encoding.UTF8).ReadToEnd();
        }
    }
}
