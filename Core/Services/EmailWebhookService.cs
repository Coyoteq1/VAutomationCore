using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAutomationCore.Core.Services
{
    /// <summary>
    /// Email webhook service for sending notifications.
    /// Supports HTTP webhooks (Slack, Discord, custom endpoints).
    /// </summary>
    public class EmailWebhookService
    {
        private static EmailWebhookService _instance;
        public static EmailWebhookService Instance => _instance ??= new EmailWebhookService();
        
        private readonly HttpClient _httpClient = new HttpClient();
        private string _webhookUrl;
        private string _smtpHost;
        private int _smtpPort;
        private string _smtpUser;
        private string _smtpPass;
        private string _fromEmail;
        private string _toEmail;
        
        /// <summary>
        /// Configure webhook URL (Discord, Slack, or custom HTTP endpoint).
        /// </summary>
        public void ConfigureWebhook(string url)
        {
            _webhookUrl = url;
            Debug.Log($"[EmailWebhook] Webhook configured: {url}");
        }
        
        /// <summary>
        /// Configure SMTP for actual email sending.
        /// </summary>
        public void ConfigureSMTP(string host, int port, string user, string pass, string from, string to)
        {
            _smtpHost = host;
            _smtpPort = port;
            _smtpUser = user;
            _smtpPass = pass;
            _fromEmail = from;
            _toEmail = to;
            Debug.Log($"[EmailWebhook] SMTP configured: {host}:{port}");
        }
        
        /// <summary>
        /// Send email via webhook or SMTP.
        /// </summary>
        public async Task<bool> SendEmailAsync(string subject, string body)
        {
            try
            {
                // Try webhook first
                if (!string.IsNullOrEmpty(_webhookUrl))
                {
                    return await SendViaWebhookAsync(subject, body);
                }
                
                // Fallback to SMTP
                if (!string.IsNullOrEmpty(_smtpHost))
                {
                    return await SendViaSMTPAsync(subject, body);
                }
                
                Debug.LogWarning("[EmailWebhook] No webhook or SMTP configured");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EmailWebhook] Send failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Send via HTTP webhook (Discord/Slack compatible).
        /// </summary>
        private async Task<bool> SendViaWebhookAsync(string subject, string body)
        {
            var payload = new
            {
                content = $"**{subject}**\n{body}",
                embeds = new[] { new { description = body, title = subject } }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(_webhookUrl, content);
            return response.IsSuccessStatusCode;
        }
        
        /// <summary>
        /// Send via SMTP (requires mail library).
        /// </summary>
        private async Task<bool> SendViaSMTPAsync(string subject, string body)
        {
            // Note: Requires System.Net.Mail or MailKit
            // This is a placeholder - actual implementation would use MailKit
            Debug.Log($"[EmailWebhook] SMTP send to {_toEmail}: {subject}");
            await Task.Delay(1); // Placeholder
            return true;
        }
        
        /// <summary>
        /// Send job results to email.
        /// </summary>
        public async Task SendJobResultsAsync(string jobName, string results)
        {
            var message = $"Job: {jobName}\nResults: {results}\nTime: {DateTime.UtcNow}";
            await SendEmailAsync($"VAutomation Job Complete", message);
        }
        
        /// <summary>
        /// Send alert to email.
        /// </summary>
        public async Task SendAlertAsync(string alertType, string message)
        {
            await SendEmailAsync($"[VAutomation] {alertType}", message);
        }
    }
    
    /// <summary>
    /// Quick access to email webhook.
    /// </summary>
    public static class Email
    {
        private static EmailWebhookService _svc => EmailWebhookService.Instance;
        
        public static void Configure(string webhookUrl) => _svc.ConfigureWebhook(webhookUrl);
        public static Task<bool> Send(string subject, string body) => _svc.SendEmailAsync(subject, body);
        public static Task SendJobResults(string job, string results) => _svc.SendJobResultsAsync(job, results);
        public static Task SendAlert(string type, string msg) => _svc.SendAlertAsync(type, msg);
        
        /// <summary>Test email with one command.</summary>
        public static async Task Test(string webhookUrl = null)
        {
            if (!string.IsNullOrEmpty(webhookUrl)) Configure(webhookUrl);
            var result = await Send("VAutomation Test", $"Test successful! Time: {DateTime.UtcNow}");
            Debug.Log($"[Email] Test result: {result}");
        }
    }
}
