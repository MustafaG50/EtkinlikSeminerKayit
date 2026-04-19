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
                var visibleIndex = 0;

                $.each(fields, function (i, field) {
                    var fieldName = field.name || field.Name;
                    var fieldId = field.id || field.Id;

                    if (fieldName.toLowerCase().includes("koltuk")) {
                        return true; 
                    }
                    

                    var html = `
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold">${fieldName}</label>
                            <input name="DynamicValues[${visibleIndex}].Value" class="form-control" required />
                            <input type="hidden" name="DynamicValues[${visibleIndex}].EventFieldId" value="${fieldId}" />
                        </div>`;

                    container.append(html);
                    visibleIndex++;
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