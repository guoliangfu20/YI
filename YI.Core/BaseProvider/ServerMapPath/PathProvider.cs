using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Extensions;
using YI.Core.Extensions.AutofacManager;

namespace YI.Core.BaseProvider.ServerMapPath
{
    public interface IPathProvider : IDependency
    {
        string MapPath(string path);
        string MapPath(string path, bool rootPath);
        IWebHostEnvironment GetHostingEnvironment();
    }


    public class PathProvider : IPathProvider
    {
        private IWebHostEnvironment _hostingEnvironment;

        public PathProvider(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }


        public IWebHostEnvironment GetHostingEnvironment()
        {
            return this._hostingEnvironment;
        }

        public string MapPath(string path)
        {
            return MapPath(path, false);
        }

        public string MapPath(string path, bool rootPath)
        {
            //if (rootPath)
            //{
            //    if (_hostingEnvironment.WebRootPath == null)
            //    {
            //        _hostingEnvironment.WebRootPath = _hostingEnvironment.ContentRootPath + "/wwwroot".ReplacePath();
            //    }
            //    return Path.Combine(_hostingEnvironment.WebRootPath, path).ReplacePath();
            //}
            //return Path.Combine(_hostingEnvironment.ContentRootPath, path).ReplacePath();


            if (rootPath && _hostingEnvironment.WebRootPath == null)
            {
                _hostingEnvironment.WebRootPath = _hostingEnvironment.ContentRootPath + "/wwwroot".ReplacePath();
            }
            return Path.Combine(_hostingEnvironment.ContentRootPath, path).ReplacePath();
        }
    }

}
