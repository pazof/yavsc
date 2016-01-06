(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.geocode', {
    inputctl: '',
    options: {
      disabled: false,
      map: 'map',
      start: { lat: -34.397, lng: 150.644 },
      zoom: 16,
      submit: 'submit',
      onValidated: null,
      },
    geocodeAddress: function (geocoder, resultsMap) { 
      var address = $(this.element).val();
      var validation = this.options.onValidated;

      geocoder.geocode({'address': address}, function(results, status) {
      if (status === google.maps.GeocoderStatus.OK) {
        resultsMap.setCenter(results[0].geometry.location);
        var pos = results[0].geometry.location;
        var marker = new google.maps.Marker({
          map: resultsMap,
          position: pos
        });
        if (validation) validation(pos);
      } else {
        alert('Geocode was not successful for the following reason: ' + status);
      }});
      },
    _create: function() {
      var _this = this;
      var $this = $(this);
	  if (!_this.options.disabled) {
        var gmap = new google.maps.Map(document.getElementById(_this.options.map), {
	    zoom: _this.options.zoom,
	    center: _this.options.start
	  });
	  var geocoder = new google.maps.Geocoder();
	  var geocodeMethod = _this.geocodeAddress;
	  var btn = document.getElementById(_this.options.submit);
	  btn.addEventListener('click', function() {
	    _this.geocodeAddress(geocoder, gmap);
	   });
	   btn.disabled = false;
	  }},

	  });
  })(jQuery);

}).call(this);



