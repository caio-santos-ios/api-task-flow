using to_do_list.src.Interfaces;
using to_do_list.src.Interfaces.INotification;
using to_do_list.src.Models;
using to_do_list.src.Models.Base;
using to_do_list.src.Requests;
using to_do_list.src.Shared.DTOs;
using to_do_list.src.Shared.Utils;

namespace to_do_list.src.Services
{
    public class TaskService(
        ITaskRepository repository,
        IUserService userService,
        INotificationService notificationService
    ) : ITaskService
    {
        #region READ
        public async Task<ResponseApi<PaginationApi<List<dynamic>>>> GetAllAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<to_do_list.src.Models.Task> pagination = new(request.QueryParams);
                ResponseApi<List<dynamic>> tasks = await repository.GetAllAsync(pagination);
                int count = await repository.GetCountDocumentsAsync(pagination);
                PaginationApi<List<dynamic>> data = new(tasks.Data, count, pagination.PageNumber, pagination.PageSize);
                return new(data, 200, "Tarefas listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<dynamic?>> GetByIdAggregateAsync(string id)
        {
            try
            {
                ResponseApi<dynamic?> task = await repository.GetByIdAggregateAsync(id);
                if (task.Data is null) return new(null, 404, "Tarefa não encontrado");
                return new(task.Data, 200, "Tarefa encontrado");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<to_do_list.src.Models.Task?>> CreateAsync(CreateTaskRequest request)
        {
            try
            {
                dynamic access = Util.GenerateCodeAccess();

                to_do_list.src.Models.Task task = new()
                {
                    Title = request.Title.ToUpper(),
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Priority = request.Priority.ToUpper(),
                    Status = "PENDENTE",
                    CategoryId = request.CategoryId,
                    CreatedBy = request.CreatedBy
                };

                ResponseApi<to_do_list.src.Models.Task?> response = await repository.CreateAsync(task);
                if (response.Data is null) return new(null, 400, "Falha ao criar conta.");

                ResponseApi<List<User>> users = await userService.GetNotByIdAsync(request.CreatedBy);
                if (users.Data is null) return new(null, 400, "Falha ao criar conta.");
                foreach (User user in users.Data)
                {
                    await notificationService.CreateAsync(new()
                    {
                        Title = "Nova Tarefa",
                        Description = $"O usuário {user.Name} criou uma nova tarefa",
                        SendBy = user.Id,
                        Icon = "task",
                        Link = $"tasks/{response.Data.Id}/details"
                    });
                }


                return new(null, 201, "Tarefa criado com sucesso.");
            }
            catch
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }

        #endregion

        #region UPDATE
        public async Task<ResponseApi<to_do_list.src.Models.Task?>> UpdateAsync(UpdateTaskRequest request)
        {
            try
            {
                ResponseApi<to_do_list.src.Models.Task?> task = await repository.GetByIdAsync(request.Id);
                if (task.Data is null) return new(null, 404, "Falha ao atualizar");

                task.Data.UpdatedAt = DateTime.UtcNow;
                task.Data.Title = request.Title.ToUpper();
                task.Data.StartDate = request.StartDate;
                task.Data.EndDate = request.EndDate;
                task.Data.Description = request.Description.ToUpper();
                task.Data.Priority = request.Priority.ToUpper();
                task.Data.CategoryId = request.CategoryId;

                ResponseApi<to_do_list.src.Models.Task?> response = await repository.UpdateAsync(task.Data);
                if (!response.IsSuccess) return new(null, 400, "Falha ao atualizar");

                return new(response.Data, 200, "Atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        public async Task<ResponseApi<to_do_list.src.Models.Task?>> FinishAsync(FinishTaskRequest request)
        {
            try
            {
                ResponseApi<to_do_list.src.Models.Task?> task = await repository.GetByIdAsync(request.Id);
                if (task.Data is null) return new(null, 404, "Falha ao finalizar");

                task.Data.UpdatedAt = DateTime.UtcNow;
                task.Data.Status = "FINALIZADO";

                ResponseApi<to_do_list.src.Models.Task?> response = await repository.UpdateAsync(task.Data);
                if (!response.IsSuccess) return new(null, 400, "Falha ao finalizar");

                return new(response.Data, 200, "Finalizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<to_do_list.src.Models.Task>> DeleteAsync(DeleteDTO request)
        {
            try
            {
                ResponseApi<to_do_list.src.Models.Task> task = await repository.DeleteAsync(request);
                if (!task.IsSuccess) return new(null, 400, task.Message);
                return new(task.Data, 204, "Tarefa excluído com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion        
    }
}