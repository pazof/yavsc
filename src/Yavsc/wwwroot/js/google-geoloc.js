if (typeof jQuery === 'undefined') {
    throw new Error('This Google maps client script requires jQuery')
}
if (typeof google === 'undefined') {
    throw new Error('This Google maps client script requires google')
}

(function($, maps) {
    $.widget('psc.googlegeocode', {
        options: {
            culture: 'fr',
            mapId: 'map',
            longId: 'Longitude',
            latId: 'Latitude',
            addrValidationId: 'AddressError',
            formValidId: 'ValidationSummary',
            locComboId: 'LocationCombo',
            specifyPlaceMsg: 'Specify a place',
            GoogleDidntGeoLocalizedMsg: 'Google didn\'t reconized this address'
        },
        marker: null,
        gmap: null,
        onDragEnd: function(_this) {
            // TODO reverse geo code
            var npos = _this.marker.getPosition();
            var nlat = Number(npos.lat());
            var nlng = Number(npos.lng());
            $('#' + _this.options.latId).val(nlat.toLocaleString(_this.options.culture, { minimumFractionDigits: 8 }));
            $('#' + _this.options.longId).val(nlng.toLocaleString(_this.options.culture, { minimumFractionDigits: 8 }));
        },
        _create: function() {
            var _this = this;
            var scenter =  { lat: parseFloat($('#' + _this.options.latId).val().replace(',', '.')),
             lng: parseFloat($('#' + _this.options.longId).val().replace(',', '.')) };
            this.element.addClass('googlegeocode');
            this.gmap = new maps.Map(document.getElementById(this.options.mapId), {
                zoom: 16,
                center: scenter
            });
            this.marker = new maps.Marker({
                map: this.gmap,
                draggable: true,
                animation: maps.Animation.DROP,
                position: scenter
            });
            maps.event.addListener(this.marker, 'dragend', function() { _this.onDragEnd(_this) });
            this.element.data('val-required', this.options.specifyPlaceMsg);
            this.element.data('val-remote', this.options.GoogleDidntGeoLocalizedMsg);
            this.element.rules('add', {
                remote: {
                    url: 'https://maps.googleapis.com/maps/api/geocode/json',
                    type: 'get',
                    data: {
                        key: _this.options.mapsApiKey,
                        sensor: false,
                        address: function() {
                            return _this.element.val();
                        }
                    },
                    dataType: 'json',
                    dataFilter: function(datastr) {
                        var ul = $('#' + _this.options.locComboId);
                        ul.html('');
                        var data = JSON.parse(datastr);
                        data.results.forEach(function(item) {
                            $('<li>' + item.formatted_address + '</li>')
                                .data('geoloc', item)
                                .click(function() { _this.chooseLoc('user', item) })
                                .css('cursor', 'pointer')
                                .appendTo(ul);
                        });
                        if ((data.status === 'OK') && (data.results.length >= 1)) {
                            return true;
                        }
                        return false;
                    }
                }
            })
        },
        chooseLoc: function(sender, loc) {
            var _this = this;
            if (sender === 'user') this.element.val(loc.formatted_address);
            var pos = loc.geometry.location;
            var lat = Number(pos.lat);
            var lng = Number(pos.lng);
            $(document.getElementById(this.options.latId)).val(lat.toLocaleString(this.options.culture, { minimumFractionDigits: 8 }));
            $(document.getElementById(this.options.longId)).val(lng.toLocaleString(this.options.culture, { minimumFractionDigits: 8 }));
            this.gmap.setCenter(pos);
            if (this.marker) {
                this.marker.setMap(null);
            }
            this.marker = new maps.Marker({
                map: this.gmap,
                draggable: true,
                animation: maps.Animation.DROP,
                position: pos
            });
            maps.event.addListener(this.marker, 'dragend', function() { _this.onDragEnd(_this) });
            this.element.valid();
            $('#' + this.options.addrValidationId).empty();
            $('#' + this.options.formValidId).empty();
            $('#' + this.options.locComboId).empty();
            return this;
        }
    })
})(jQuery, google.maps);
