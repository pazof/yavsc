(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.rate', {
    options: {
      webTarget: null,
      jsTarget: null,
      disabled: false,
      labels: [ 'médiocre','passable','moyen','bon', 'très bon' ,'excellent', 'parfait'],
      },
	sendRate: function (rating,callback) {
		if (this.options.webTarget)
			Yavsc.ajax(this.options.webTarget+'/Rate', rating, callback);
		if (this.options.jsTarget)
			if (this.options.jsTarget(rating))
			  if (callback) callback();
		},
    _create: function() {
	   var $ratectl = $(this.element); 
	   var _this = this;
	   $ratectl.onChanged = function (newrate) { 
	   		// build the five stars
	   		_this.updateRate($ratectl,newrate);
	   };
	   var id = $ratectl.data('id');
	   $ratectl.addClass('rate');
	   $ratectl.css('cursor','pointer');
	   $ratectl.click(function (e) { 
		   var oset = $ratectl.offset(); 
		   var x = ((e.pageX - oset.left) * 100 ) / ($ratectl.innerWidth());
		   // here, x may be greater than 100, (or lower than 0, i saw it),
		   // depending on padding & margin on the $ratectl node,
		   // when it's a span, and there is a line return within,
		   // the values on second star line are false.
		   // Time to sanitize:
		   x = Math.ceil(x);
			if (x<0) x = 0;
			if (x>100) x = 100;
			var data = { Id: id, Rate: x };
		   _this.sendRate(data, function () {
		   $ratectl.onChanged(x); });
		   });
		},
	updateRate: function (ctl,rate) {
	    var _this = this;
		var rounded = Math.round(rate / 10);
		var HasHalf = (rounded % 2 == 1);
		var NbFilled = Math.floor(rounded / 2);
		var NbEmpty = (5 - NbFilled) - ((HasHalf)?1:0) ;
		ctl.empty();
		var i=0;
		for (i=0; i<NbFilled; i++) 
			ctl.append('<i class="fa fa-star" title="'+_this.options.labels[i]+'"></i>');
		if (HasHalf) { i++; 
			ctl.append('<i class="fa fa-star-half-o" title="'+_this.options.labels[i]+'"></i>');
			}
		for (var j=0; j<NbEmpty; j++, i++ )
			ctl.append('<i class="fa fa-star-o" title="'+_this.options.labels[i]+'"></i>');

	},
})})(jQuery);
}).call(this);
