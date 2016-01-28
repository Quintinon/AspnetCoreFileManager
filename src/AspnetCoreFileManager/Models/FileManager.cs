//TODO: find replacement for System.Web.Exception
//TODO: is WebUtility.UrlDecode the ideal class to call from? WebUtility vs IHtmlEncoder
//TODO: read from config to set UserUploadDirecttory
//TODO: implement some kind of image filtering
//TODO: can I inject IApplicationEnvironment to simplify the constructor?
//TODO: simplify constructor FileManager(string directory, string requestPath, IApplicationEnvironment appEnvironment) to send fewer properties

using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace AspnetCoreFileManager.Models
{
    public class FileManager
    {
        private string _fileManagerPath;

        public string FileSystemPath { get; private set; }

        public string CurrentDirectory { get; private set; }

        private string _breadcrumbs;
        public string Breadcrumbs
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_breadcrumbs))
                    return _breadcrumbs;

                var crumbs = new StringBuilder();
                var url = new StringBuilder();
                int i = 0;
                var dirCount = CurrentDirectory.Length - CurrentDirectory.Replace("/", "").Length;

                //var qs = QueryHelpers.ParseQuery(querystring);
                foreach (string s in CurrentDirectory.Remove(0, 1).Split('/'))
                {
                    url.Append("/" + s);
                    i += 1;
                    if (i == dirCount)
                        crumbs.Append("<li>" + s + "</li>" + Environment.NewLine);
                    else
                    {
                        //qs.Set("dir", Server.UrlEncode(url.ToString()));
                        crumbs.Append("<li><a href=\"" + _fileManagerPath + "?dir=" + WebUtility.UrlEncode(url.ToString()) + "\">" + s + "</a></li>" + Environment.NewLine);
                    }
                }
                crumbs.Insert(0, "<ol class=\"breadcrumb\">" + Environment.NewLine);
                crumbs.Append(Environment.NewLine + "</ol>");
                _breadcrumbs = crumbs.ToString();

                return _breadcrumbs;
            }
        }

        private List<FileManagerItem> _fileManagerItems;
        public List<FileManagerItem> FileManagerItems
        {
            get
            {
                if (_fileManagerItems != null)
                    return _fileManagerItems;

                var dir = new DirectoryInfo(FileSystemPath + CurrentDirectory);
                var files = dir.GetFileSystemInfos();

                var fmFolder = new List<FileManagerItem>();
                var fmFiles = new List<FileManagerItem>();

                foreach (var fileSystemInfo in files)
                {
                    var fmi = new FileManagerItem();
                    fmi.Name = fileSystemInfo.Name;
                    fmi.LastModified = fileSystemInfo.LastWriteTime;

                    if (fileSystemInfo is DirectoryInfo)
                    {
                        //var sd = new string[1];
                        //sd.SetValue(WebUtility.UrlEncode(CurrentDirectory + "/" + f.Name), 0);
                        //qs["dir"] = sd;
                        //qs.Add("dir", WebUtility.UrlEncode(CurrentDirectory + "/" + f.Name));
                        //qs.Set("dir", Server.UrlEncode(_currentDirectory + "/" + directoryInfo.Name));

                        fmi.ItemType = FileManagerItemType.Directory;
                        fmi.BrowseUrl = _fileManagerPath + "?dir=" + WebUtility.UrlEncode(CurrentDirectory + "/" + fileSystemInfo.Name); //QueryHelpers.AddQueryString(urlLocalPath, qs);
                        fmi.DeleteUrl = _fileManagerPath + "/delete-directory?dir=" + WebUtility.UrlEncode(CurrentDirectory + "/" + fileSystemInfo.Name);
                        fmi.ThumbnailUrl = "/images/icons/125x125/folder.png";
                        fmFolder.Add(fmi);
                    }
                    else
                    {
                        fmi.ItemType = FileManagerItemType.File;
                        fmi.Size = (((FileInfo)fileSystemInfo).Length / 1000).ToString() + " KB";
                        fmi.BrowseUrl = _fileManagerPath;
                        fmi.DeleteUrl = _fileManagerPath + "/delete-file?file=" + WebUtility.UrlEncode(CurrentDirectory + "/" + fileSystemInfo.Name);
                        fmi.ThumbnailUrl = CurrentDirectory + "/" + fileSystemInfo.Name + "?width=125&height=125";
                        fmFiles.Add(fmi);
                    }
                }

                _fileManagerItems = new List<FileManagerItem>();
                _fileManagerItems.AddRange(fmFolder);
                _fileManagerItems.AddRange(fmFiles);

                return _fileManagerItems;
            }
        }

        public FileManager(string directory, string fileManagerPath, string fileSystemPath)
        {
            //var appEnv = (IApplicationEnvironment)CallContextServiceLocator.Locator.ServiceProvider.GetService(typeof(IApplicationEnvironment));

            if (!string.IsNullOrWhiteSpace(directory) && directory.StartsWith("/secure-files"))
            {
                FileSystemPath = fileSystemPath + "/Uploads";
                CurrentDirectory = directory;
            }
            else
            {
                FileSystemPath = fileSystemPath + "/wwwroot";
                CurrentDirectory = string.IsNullOrWhiteSpace(directory) ? "/media" : directory;
                if (!CurrentDirectory.StartsWith("/media"))
                {
                    //throw new System.Web.HttpException("Access is denied on the upload directory.");
                    throw new Exception("Access is denied on the upload directory.");
                }
            }

            _fileManagerPath = fileManagerPath;
        }
    }
}