using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Glow.Files
{
    public interface IFile
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Path { get; set; }
    }

    public class FileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class FileService
    {
        public async Task<IList<T>> WriteFormfilesToPath<T>(IFormFileCollection files, string path) where T : IFile
        {
            var folder = Path.Combine(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var result = new List<T>();
            foreach (IFormFile file in files)
            {
                var id = Guid.NewGuid();
                T newFile = Activator.CreateInstance<T>();
                newFile.Id = id;
                newFile.Name = file.FileName;
                newFile.Path = Path.Combine(folder, id.ToString());

                using var stream = new FileStream(newFile.Path, FileMode.Create);
                await file.CopyToAsync(stream);
                result.Add(newFile);
            }
            return result;
        }
    }
}
