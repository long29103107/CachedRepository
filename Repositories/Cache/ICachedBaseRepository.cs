using CachedRepository.Entities;
using CachedRepository.Repositories.Base;

namespace CachedRepository.Repositories.Cache;

public interface ICachedBaseRepository<T> : IBaseRepository<T> where T : BaseEntity;