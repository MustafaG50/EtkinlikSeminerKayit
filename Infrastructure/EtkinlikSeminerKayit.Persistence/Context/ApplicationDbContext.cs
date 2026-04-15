using EtkinlikSeminerKayit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Resource> Resources { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<EventField> EventFields { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<EventValue> EventValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EAV Performans İyileştirmesi: EventValues tablosunda hızlı arama için index
            modelBuilder.Entity<EventValue>()
                .HasIndex(v => v.ReservationId);

            // Çakışma kontrollerinde ResourceId ve Tarih aralıkları sık sorgulanacak
            modelBuilder.Entity<Reservation>()
                .HasIndex(r => new { r.ResourceId, r.StartTime, r.EndTime });

            base.OnModelCreating(modelBuilder);

            // EventValue silinirken Reservation silinmesin, sadece bağ kopsun (NoAction)
            modelBuilder.Entity<EventValue>()
                .HasOne(ev => ev.Reservation)
                .WithMany(r => r.EventValues)
                .HasForeignKey(ev => ev.ReservationId)
                .OnDelete(DeleteBehavior.NoAction); // Kritik satır burası

            // Eğer aynı hata EventField için de gelirse bunu da ekleyebilirsin:
            modelBuilder.Entity<EventValue>()
                .HasOne(ev => ev.EventField)
                .WithMany()
                .HasForeignKey(ev => ev.EventFieldId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
