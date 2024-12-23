using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using minio.Domain.entity;

namespace minio.Infrastructure.Repositories
{
    public interface IFileRepository
    {
        Task AddAsync(FileEntity entity);
        Task UpdateAsync(FileEntity entity);
        FileEntity GetById(Guid id);
        Task<IEnumerable<FileEntity>> FindByDeletedAsync(bool deleted);
        Task<FileEntity> FindByUniqueKeyAsync(string uniqueKey);
    }
}

