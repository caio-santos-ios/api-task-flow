using to_do_list.src.Infraestructure;
using to_do_list.src.Interfaces;
using to_do_list.src.Models.Base;
using to_do_list.src.Shared.DTOs;
using to_do_list.src.Shared.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace to_do_list.src.Repository
{
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        #region CREATE
        public async Task<ResponseApi<to_do_list.src.Models.Category?>> CreateAsync(to_do_list.src.Models.Category category)
        {
            try
            {
                await context.Categories.InsertOneAsync(category);
                return new(category, 201, "Categoria criado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");   
            }
        }
        #endregion
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(PaginationUtil<to_do_list.src.Models.Category> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$skip", pagination.Skip),
                    new("$limit", pagination.Limit),
                    
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"code", 1},
                        {"active", 1}
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Categories.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(PaginationUtil<to_do_list.src.Models.Category> pagination)
        {
            try
            {
                List<BsonDocument> pipeline = new()
                {
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"code", 1},
                        {"active", 1}
                    }),
                    new("$sort", pagination.PipelineSort),
                };

                List<BsonDocument> results = await context.Categories.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
                return new(list);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                BsonDocument[] pipeline = [
                    new("$match", new BsonDocument{
                        {"_id", new ObjectId(id)},
                        {"deleted", false}
                    }),

                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"code", 1}
                    }),
                ];

                BsonDocument? response = await context.Categories.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                dynamic? result = response is null ? null : BsonSerializer.Deserialize<dynamic>(response);
                return result is null ? new(null, 404, "Categoria não encontrado") : new(result);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<to_do_list.src.Models.Category?>> GetByIdAsync(string id)
        {
            try
            {
                to_do_list.src.Models.Category? category = await context.Categories.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(category);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<long> GetNextCode(string userId)
        {
            return await context.Categories.Find(x => x.CreatedBy == userId).CountDocumentsAsync() + 1;
        }
        public async Task<int> GetCountDocumentsAsync(PaginationUtil<to_do_list.src.Models.Category> pagination)
        {
            List<BsonDocument> pipeline = new()
            {
                new("$match", pagination.PipelineFilter),
                new("$sort", pagination.PipelineSort),
                new("$addFields", new BsonDocument
                {
                    {"id", new BsonDocument("$toString", "$_id")},
                }),
                new("$project", new BsonDocument
                {
                    {"_id", 0},
                    {"password", 0},
                    {"role", 0},
                    {"blocked", 0},
                    {"codeAccess", 0},
                    {"validatedAccess", 0}
                }),
                new("$sort", pagination.PipelineSort),
            };

            List<BsonDocument> results = await context.Categories.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }
        #endregion
        #region UPDATE
        public async Task<ResponseApi<to_do_list.src.Models.Category?>> UpdateAsync(to_do_list.src.Models.Category category)
        {
            try
            {
                await context.Categories.ReplaceOneAsync(x => x.Id == category.Id, category);
                return new(category, 200, "Categoria atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
        #region DELETE
        public async Task<ResponseApi<to_do_list.src.Models.Category>> DeleteAsync(DeleteDTO request)
        {
            try
            {
                to_do_list.src.Models.Category? category = await context.Categories.Find(x => x.Id == request.Id && !x.Deleted).FirstOrDefaultAsync();
                if(category is null) return new(null, 404, "Categoria não encontrado");
                
                category.Deleted = true;
                category.DeletedAt = DateTime.UtcNow;
                category.DeletedBy = request.DeletedBy;

                await context.Categories.ReplaceOneAsync(x => x.Id == category.Id, category);

                return new(category, 204, "Categoria excluído com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        #endregion
    }
}