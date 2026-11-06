using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//interfaz devuelve un resultado de una consulta not null, es decir, siempre devuelve un resultado
//response no puede ser nulos
namespace BuildingBlocks.CQRS
{
    public interface IQuery<out TResponse>:IRequest<TResponse>
        where TResponse : notnull
    {
    }
}
