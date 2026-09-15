using AgitekBackend.DTOs.AuditLogs;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class AuditLogMapper : AutoMapper.Profile
    {
        public AuditLogMapper()
        {
            CreateMap<CreateAuditLogDTO, AuditLog>();
            CreateMap<UpdateAuditLogDTO, AuditLog>();
            CreateMap<AuditLog, AuditLogDTO>();
        }
    }
}



