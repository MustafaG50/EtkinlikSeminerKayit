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
        //bizim kullanacağımız ayarları alır ve DbContext e iletir.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Resource> Resources { get; set; } // Tablolar 
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<EventField> EventFields { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<EventValue> EventValues { get; set; }

        //DB oluşturulurken EF Core verdiğimiz talimatlar.
        //Bir kere db bağlantısı yaparken çalışır.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // İndeksleme yaparak EventValue tablosunda ReservationId üzerinden hızlı sorgulama sağlanır
            modelBuilder.Entity<EventValue>()
                .HasIndex(v => v.ReservationId);

            // Çakışma kontrolü için sık sorgulanacak alanlara indeke ekledik.
            modelBuilder.Entity<Reservation>()
                .HasIndex(r => new { r.ResourceId, r.StartTime, r.EndTime });

            base.OnModelCreating(modelBuilder);//önce standart ayarlar uygulanır sonra benimkiler.

            // EventValue silinirken rezervasyonun silinmesini engeller.
            modelBuilder.Entity<EventValue>()
                .HasOne(ev => ev.Reservation)
                .WithMany(r => r.EventValues)
                .HasForeignKey(ev => ev.ReservationId)
                .OnDelete(DeleteBehavior.NoAction); // Kritik satır

            // EventValue silinirken EventField'ın silinmesini engeller.
            modelBuilder.Entity<EventValue>()
                .HasOne(ev => ev.EventField)
                .WithMany()
                .HasForeignKey(ev => ev.EventFieldId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
