using to_do_list.src.Models.Base;
using to_do_list.src.Shared.DTOs;
using to_do_list.src.Shared.Utils;

namespace to_do_list.src.Interfaces
{
    public interface ICategoryRepository
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<to_do_list.src.Models.Category> pagination);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(PaginationUtil<to_do_list.src.Models.Category> pagination);
        Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id);
        Task<ResponseApi<to_do_list.src.Models.Category?>> GetByIdAsync(string id);
        Task<long> GetNextCode(string userId);
        Task<int> GetCountDocumentsAsync(PaginationUtil<to_do_list.src.Models.Category> pagination);
        Task<ResponseApi<to_do_list.src.Models.Category?>> CreateAsync(to_do_list.src.Models.Category user);
        Task<ResponseApi<to_do_list.src.Models.Category?>> UpdateAsync(to_do_list.src.Models.Category request);
        Task<ResponseApi<to_do_list.src.Models.Category>> DeleteAsync(DeleteDTO request);
    }
}