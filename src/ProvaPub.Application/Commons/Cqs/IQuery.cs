using MediatR;

namespace ProvaPub.Application.Commons.Cqs;

public interface IQuery<out TResponse> : IRequest<TResponse>
{ }