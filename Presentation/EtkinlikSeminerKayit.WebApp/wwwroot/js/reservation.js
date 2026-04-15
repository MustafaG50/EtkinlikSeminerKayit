$(document).ready(function () {
    console.log("Reservation JS aktif.");

    function loadDynamicFields(eventTypeId) {
        var container = $('#dynamicFieldsContainer');

        if (!eventTypeId) {
            container.html('<div class="col-12"><p class="text-muted italic">Lütfen etkinlik türü seçin...</p></div>');
            return;
        }

        $.getJSON('/Reservation/GetFieldsByEventType', { eventTypeId: eventTypeId }, function (fields) {
            console.log("API'den Gelen Ham Veri:", fields);
            container.empty();

            if (fields && fields.length > 0) {
                // SAYAÇ EKLEDİK: i parametresi yerine kendi sayacımızı kullanmalıyız 
                // çünkü bazı alanları atladığımızda index bozulmamalı.
                var visibleIndex = 0;

                $.each(fields, function (i, field) {
                    var fieldName = field.name || field.Name;
                    var fieldId = field.id || field.Id;

                    // --- DEĞİŞİKLİK BURADA ---
                    // Eğer alan adı "Koltuk" içeriyorsa bu döngü adımını atla (ekrana basma)
                    if (fieldName.toLowerCase().includes("koltuk")) {
                        return true; // jQuery each içinde 'continue' demektir.
                    }
                    // -------------------------

                    var html = `
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold">${fieldName}</label>
                            <input name="DynamicValues[${visibleIndex}].Value" class="form-control" required />
                            <input type="hidden" name="DynamicValues[${visibleIndex}].EventFieldId" value="${fieldId}" />
                        </div>`;

                    container.append(html);
                    visibleIndex++; // Sadece ekrana basılan alanlar için indexi artır
                });

                console.log("Kutular HTML'e eklendi.");
            } else {
                container.html('<div class="col-12"><p class="text-warning">Ek alan bulunamadı.</p></div>');
            }
        });
    }

    $('#EventTypeId').on('change', function () {
        loadDynamicFields($(this).val());
    });

    var currentId = $('#EventTypeId').val();
    if (currentId) { loadDynamicFields(currentId); }
});