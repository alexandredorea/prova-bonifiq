using MediatR;

namespace ProvaPub.Application.Commons.Cqs;

public interface ICommand<out TResponse> : IRequest<TResponse>
{ }