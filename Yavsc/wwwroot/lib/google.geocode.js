(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.geocode', {
    geocoder:null,
    gmap:null,  
    combo:null,
    inputLatitude:null,
    inputLongitude:null,
    marker:null,
    options: {
        disabled: false,
        map: 'map',
        start: { lat: -34.397, lng: 150.644 },
        zoom: 16,
        btngeoloc: null,
        combo: 'Location_combo',
        inputLatitude: 'Location_Longitude',
        inputLongitude: 'Location_Latitude',
        onValidated: null,
        onProposalClicked: null
      },
      onBlur: function()
      {
          this.geocodeAddress();
      },
    geocodeAddress: function () {
      var validation = this.options.onValidated;
      var _this = this;
      var $input = $(this.element);
      var address = $input.val();
      $input.data("isRequesting", true);
      $input.data("validPlaceId", false);
      _this.geocoder.geocode({'address': address}, function(results, status) {
      if (status === google.maps.GeocoderStatus.OK) {
        var firstres = results[0];
        var pos = firstres.geometry.location;
        if (_this.combo) {
            var $combo = $(_this.combo);
            $combo.html("");
            if (results.length>1) {
            results.forEach(function(element) {
                $('<li>'+element.formatted_address+'</li>')
                .click( function() { 
                    $(_this.element).val($(this).text()); 
                    if (_this.options.onProposalClicked)
                       _this.options.onProposalClicked();
                    } )
                .appendTo($combo);
            },this);
            }
        }
        if (_this.marker) { 
            _this.marker.setMap(null);
            _this.marker = null;
        }
        if (results.length==1) {
            $(_this.element).val(firstres.formatted_address);
            _this.gmap.setCenter(pos);
            _this.marker = new google.maps.Marker({
            map: _this.gmap,
            draggable: true,
            animation: google.maps.Animation.DROP,
            position: pos
            });
            
            if (_this.inputLatitude) _this.inputLatitude.value=pos.lat;
            if (_this.inputLongitude) _this.inputLongitude.value = pos.lng;
            if (validation) validation({results:results,success:true});
            $input.data("validPlaceId", true);
            $input.data("isRequesting", false);
            if (validation) validation({success:true,status: status});
            return;
        } }
        if (validation) validation({success:false,status: status});
        $input.data("isRequesting", false);
       } );
      },
    _create: function() {
      var _this = this;
      var $input = $(this.element);
	  if (!_this.options.disabled) {
        _this.combo = document.getElementById(this.options.combo);
        _this.gmap = new google.maps.Map(document.getElementById(_this.options.map), {
	    zoom: _this.options.zoom,
	    center: _this.options.start
	  });
      if (_this.options.inputLatitude)
       _this.inputLatitude = document.getElementById(_this.options.inputLatitude);
      if (_this.options.inputLongitude)
       _this.inputLongitude = document.getElementById(_this.options.inputLongitude);
       
	  this.geocoder = new google.maps.Geocoder();
	  var btn = document.getElementById(_this.options.btngeoloc);
      if (btn) {
	       btn.click( function() {
	    _this.geocodeAddress();
	   });
	   btn.disabled = false;
      }
      $input.on('blur',function() {_this.onBlur(); });
      }},
      getDistance: function(lat1,lon1,lat2,lon2){
  var R = 6371; // Earth's radius in Km
  return Math.acos(Math.sin(lat1)*Math.sin(lat2) + 
                  Math.cos(lat1)*Math.cos(lat2) *
                  Math.cos(lon2-lon1)) * R;
}
	  });
  })(jQuery);

}).call(this);
