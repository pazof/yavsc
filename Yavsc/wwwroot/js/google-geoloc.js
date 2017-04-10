+(function($,maps){
$.widget("psc.googlegeocode" , {
    options: {
        mapId: 'map',
        longId: 'Longitude',
        latId: 'Latitude',
        addrValidationId: 'AddressError',
        formValidId: 'ValidationSummary',
        locComboId: 'LocationCombo'
    },
     marker: null,
     gmap: null,

     _create: function() {
         this.element.addClass("googlegeocode");
         this.gmap = new maps.Map(document.getElementById(this.options.mapId), {
            zoom: 16,
            center: { lat: 48.862854, lng: 2.2056466 }
	    });
         var _this =this;
         this.element.rules("add",
    {
        remote: {
            url: 'https://maps.googleapis.com/maps/api/geocode/json',
            type: 'get',
            data: {
                sensor: false,
                address: function () { return _this.element.val() }
                },
            dataType: 'json',
            dataFilter: function(datastr) {
                $('#'+_this.options.locComboId).html("");
                var data = JSON.parse(datastr);
                data.results.forEach(function(item) {
                    if (item.formatted_address !== _this.element.val()) {
                        $('<li>'+item.formatted_address+'</li>')
                        .data("geoloc",item)
                        .click(function() { _this.chooseLoc('user',item) })
                        .css('cursor','pointer')
                        .appendTo($('#'+_this.options.locComboId));}
                    else {  }
                });
               if  ((data.status === 'OK') && (data.results.length == 1))
                { 
             //       _this.chooseLoc('google',data.results[0]);
                    return true;
                } 
                return false;
                },
            error: function()
                {
                // xhr, textStatus, errorThrown console.log('ajax loading error ... '+textStatus+' ... '+ errorThrown);
                return false;
                }
         }
        })},
        chooseLoc: function(sender,loc) {
        if (sender === 'user') this.element.val(loc.formatted_address);
        var pos = loc.geometry.location;
        var lat = new Number(pos.lat);
        var lng = new Number(pos.lng);
        $(document.getElementById(this.options.latId)).val(lat.toLocaleString('en'));
        $(document.getElementById(this.options.longId)).val(lng.toLocaleString('en'));
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
        maps.event.addListener(this.marker, 'dragend', function() {
            // TODO reverse geo code
            var pos = this.marker.getPosition();
                $('#'+this.options.latId).val(pos.lat);
                $('#'+this.options.longId).val(pos.lng);
        });
        this.element.valid();
        $('#'+this.options.addrValidationId).empty();
        $('#'+this.options.formValidId).empty();
        $('#'+this.options.locComboId).empty();
        
        return this;
    }
})
})(jQuery,google.maps)
