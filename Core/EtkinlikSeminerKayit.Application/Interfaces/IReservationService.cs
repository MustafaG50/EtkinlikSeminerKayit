using EtkinlikSeminerKayit.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Application.Interfaces
{
    public interface IReservationService
    {
        // DTO alır, kontrol eder ve giriş izni verir.
        Task<(bool IsSuccess, string Message)> CreateReservationAsync(CreateReservationDto dto);
    }
}
