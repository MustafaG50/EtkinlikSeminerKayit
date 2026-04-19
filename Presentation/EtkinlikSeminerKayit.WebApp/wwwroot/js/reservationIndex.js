function confirmBulkDelete(groupId, salonName) {
    Swal.fire({
        title: 'Tüm Salon Boşaltılsın mı?',
        text: salonName + " için bu saatteki TÜM kayıtlar silinecek!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Evet, Hepsini Sil',
        cancelButtonText: 'Vazgeç'
    }).then((result) => {
        if (result.isConfirmed) document.getElementById('bulk-delete-' + groupId).submit();
    });
}

function confirmDelete(id) {
    Swal.fire({
        title: 'Emin misiniz?',
        text: "Bu koltuk rezervasyonu silinecektir.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Sil',
        cancelButtonText: 'Vazgeç'
    }).then((result) => {
        if (result.isConfirmed) document.getElementById('delete-form-' + id).submit();
    });
}

function confirmAdd(id, seat) {
    Swal.fire({
        title: 'Koltuk Rezerve Et',
        text: seat + " numaralı koltuk eklensin mi?",
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Ekle'
    }).then((result) => {
        if (result.isConfirmed) document.getElementById('add-form-' + id).submit();
    });
}