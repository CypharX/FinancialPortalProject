﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Services
{
    public class FP_FileService : IFP_FileService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            MemoryStream memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var byteFile = memoryStream.ToArray();
            memoryStream.Close();
            memoryStream.Dispose();

            return byteFile;

        }

        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            string imageBase64Data = Convert.ToBase64String(fileData);
            return string.Format($"data:image/{extension};base64,{imageBase64Data}");

        }

        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");
            return $"/Images/png/{ext}.png";
        }

        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        public async Task<byte[]> AssignDefaultAvatarAsync(string avatar)
        {
            var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/img", avatar);
            return await File.ReadAllBytesAsync(avatarPath);
        }
    }
}
