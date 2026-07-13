using AutoMapper;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.DTOs;

namespace CustomerPayments.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<Payment, PaymentDto>();
    }
}