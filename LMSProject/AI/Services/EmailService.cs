using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace LMSProject.AI.Services
{
    public class EmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _ssl;
        private readonly string _from;
        private readonly string _fromName;
        private readonly string _password;

        public EmailService(IConfiguration cfg)
        {
            _host = cfg["Email:SmtpHost"] ?? "smtp.gmail.com";
            _port = int.Parse(cfg["Email:SmtpPort"] ?? "587");
            _ssl = bool.Parse(cfg["Email:UseSsl"] ?? "true");
            _from = cfg["Email:SenderEmail"] ?? "nm5805018@gmail.com";
            _fromName = cfg["Email:SenderName"] ?? "TOTC LMS";
            _password = cfg["Email:Password"] ?? "rcun slee vrca gymp";
        }

        // ── Core send ──────────────────────────────────────────────────────
        public async Task SendAsync(string toEmail, string toName,
            string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_fromName, _from));
            msg.To.Add(new MailboxAddress(toName, toEmail));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_host, _port,
                _ssl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            await smtp.AuthenticateAsync(_from, _password);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }

        // ── Welcome email ──────────────────────────────────────────────────
        public async Task SendWelcomeEmailAsync(string toEmail, string fullName,
            string role, string tempPassword)
        {
            var html = BuildWelcomeHtml(fullName, toEmail, role, tempPassword);
            await SendAsync(toEmail, fullName,
                "Welcome to TOTC LMS — Your Account Details", html);
        }

        // ── Weekly parent report email ─────────────────────────────────────
        public async Task SendWeeklyReportAsync(string parentEmail, string parentName,
            string studentName, string reportHtml)
        {
            var html = BuildWeeklyReportHtml(parentName, studentName, reportHtml);
            await SendAsync(parentEmail, parentName,
                $"Weekly Report: {studentName} — {DateTime.Now:MMM dd}", html);
        }

        // ── HTML builders — using plain string concatenation avoids all
        //    C# raw-string interpolation vs CSS brace/dash conflicts ─────────
        private static string BuildWelcomeHtml(string fullName, string email,
            string role, string tempPassword)
        {
            return
                "<!DOCTYPE html><html><head><meta charset='utf-8'/><style>" +
                "body{font-family:'Segoe UI',sans-serif;background:#f4f6fb;margin:0;padding:0}" +
                ".container{max-width:520px;margin:40px auto;background:white;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(47,50,125,.1)}" +
                ".header{background:linear-gradient(135deg,#2F327D,#5B72EE);padding:2rem;text-align:center}" +
                ".header h1{color:white;font-size:1.5rem;margin:.5rem 0 0}" +
                ".header small{color:rgba(255,255,255,.75);font-size:.85rem}" +
                ".body{padding:2rem}" +
                ".body p{color:#374151;font-size:.9rem;line-height:1.7}" +
                ".credentials{background:#f8f9ff;border:1.5px solid rgba(47,50,125,.12);border-radius:12px;padding:1.25rem;margin:1.25rem 0}" +
                ".cred-row{display:flex;justify-content:space-between;margin-bottom:.5rem;font-size:.88rem}" +
                ".cred-label{color:#9ca3af;font-weight:600}" +
                ".cred-val{color:#2F327D;font-weight:700}" +
                ".btn{display:block;text-align:center;background:#00CBB8;color:white;text-decoration:none;padding:.8rem 1.5rem;border-radius:10px;font-weight:700;margin:1.5rem auto;width:fit-content}" +
                ".footer{text-align:center;padding:1rem;color:#9ca3af;font-size:.78rem}" +
                "</style></head><body>" +
                "<div class='container'>" +
                "<div class='header'><div style='font-size:2rem;'>🎓</div><h1>Welcome to TOTC LMS</h1><small>Your account has been created</small></div>" +
                "<div class='body'>" +
                $"<p>Hi <strong>{Esc(fullName)}</strong>,</p>" +
                $"<p>An account has been created for you on the TOTC Learning Management System. Your role is <strong>{Esc(role)}</strong>.</p>" +
                "<div class='credentials'>" +
                $"<div class='cred-row'><span class='cred-label'>Email</span><span class='cred-val'>{Esc(email)}</span></div>" +
                $"<div class='cred-row'><span class='cred-label'>Temporary Password</span><span class='cred-val'>{Esc(tempPassword)}</span></div>" +
                "</div>" +
                "<p>Please log in and change your password immediately for security.</p>" +
                "<a class='btn' href='#'>Sign in to TOTC LMS</a>" +
                "<p style='color:#9ca3af;font-size:.78rem;'>If you did not expect this email, contact your school administrator.</p>" +
                "</div>" +
                "<div class='footer'>© TOTC LMS — All rights reserved</div>" +
                "</div></body></html>";
        }

        private static string BuildWeeklyReportHtml(string parentName,
            string studentName, string reportHtml)
        {
            return
                "<!DOCTYPE html><html><head><meta charset='utf-8'/><style>" +
                "body{font-family:'Segoe UI',sans-serif;background:#f4f6fb;margin:0}" +
                ".container{max-width:600px;margin:30px auto;background:white;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08)}" +
                ".header{background:linear-gradient(135deg,#00CBB8,#29B9E7);padding:1.75rem 2rem}" +
                ".header h2{color:white;margin:0;font-size:1.3rem}" +
                ".header small{color:rgba(255,255,255,.8);font-size:.82rem}" +
                ".body{padding:1.75rem 2rem;color:#374151;font-size:.9rem;line-height:1.7}" +
                ".footer{text-align:center;padding:1rem;color:#9ca3af;font-size:.75rem}" +
                "</style></head><body>" +
                "<div class='container'>" +
                $"<div class='header'><h2>📋 Weekly Progress Report</h2><small>{Esc(studentName)} — Week of {DateTime.Now:MMMM dd, yyyy}</small></div>" +
                "<div class='body'>" +
                $"<p>Dear <strong>{Esc(parentName)}</strong>,</p>" +
                $"<p>Here is this week's academic progress report for <strong>{Esc(studentName)}</strong>:</p>" +
                reportHtml +
                "</div>" +
                "<div class='footer'>TOTC LMS — Automated Weekly Report</div>" +
                "</div></body></html>";
        }

        private static string Esc(string s)
            => System.Net.WebUtility.HtmlEncode(s);
    }
}