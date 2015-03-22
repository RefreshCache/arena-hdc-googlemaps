function getObjectClass(obj) {
    if (obj && obj.constructor && obj.constructor.toString) {
        var arr = obj.constructor.toString().match(
            /function\s*(\w+)/);

        if (arr && arr.length == 2) {
            return arr[1];
        }
    }

    return undefined;
}

var Address = function(street, city, state, postal) {
    this.Street = street;
    this.City = city;
    this.State = state;
    this.Postal = postal;
};

var GeoAddress = function(latitude, longitude) {
    this.Latitude = latitude;
    this.Longitude = longitude;
};

function GenericMarker(opts, uniqueID) {
    var obj = new google.maps.Marker(opts);
    obj.uniqueID = uniqueID;
    obj.googlemap = null;
    return obj;
};
GenericMarker.prototype = new google.maps.Marker;

function PersonMarker(opts, personID) {
    var obj = new google.maps.Marker(opts);
    obj.personID = personID;
    obj.googlemap = null;
    return obj;
};
PersonMarker.prototype = new google.maps.Marker;

function FamilyMarker(opts, familyID) {
    var obj = new google.maps.Marker(opts);
    obj.familyID = familyID;
    obj.googlemap = null;
    return obj;
};
FamilyMarker.prototype = new google.maps.Marker;

function GroupMarker(opts, groupID) {
    var obj = new google.maps.Marker(opts);
    obj.groupID = groupID;
    obj.googlemap = null;
    return obj;
};
GroupMarker.prototype = new google.maps.Marker;

var GoogleMapRoot = "UserControls/Custom/HDC/GoogleMaps/";

var GoogleMap = function (element, geo, url, options) {
    //
    // Setup the google maps options.
    //
    defaultOptions = {
        zoom: 12,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    if (typeof (options) == 'object')
        options = $.extend(defaultOptions, options);
    else
        options = defaultOptions;
    options.center = new google.maps.LatLng(geo.Latitude, geo.Longitude);

    //
    // Setup object properties.
    //
    this.map = new google.maps.Map(document.getElementById(element), options);

    this.arenaurl = url;
    if (this.arenaurl[this.arenaurl.length - 1] != '/')
        this.arenaurl = this.arenaurl + '/';
    this.serviceurl = url + GoogleMapRoot + "GoogleService.asmx";
    this.custompins = new Array();
    this.infowindow = null;
    this.workingDiv = $('<div style="position: absolute; z-index: 10;"><img src="' + GoogleMapRoot + 'Images/ajax-bar.gif" /></div>');
    this.workingCount = 0;

    var mapDiv = $('#' + element);
    this.workingDiv.hide();
    this.workingDiv.css('left', (mapDiv.width() - 220) / 2);
    this.workingDiv.css('top', mapDiv.height() / 2);
    mapDiv.append(this.workingDiv);


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadPeopleInGeoRadius = function (geo, distance, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddPeopleToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadPeopleInRadius", { latitude: geo.Latitude, longitude: geo.Longitude, distance: distance }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of FamilyPlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadFamiliesInGeoRadius = function (geo, distance, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddFamiliesToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadFamiliesInRadius", { latitude: geo.Latitude, longitude: geo.Longitude, distance: distance }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of GroupPlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadGroupsInGeoRadius = function (geo, distance, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddGroupsToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadGroupsInRadius", { latitude: geo.Latitude, longitude: geo.Longitude, distance: distance }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadPeopleInArea = function (areaid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddPeopleToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadPeopleInArea", { areaid: areaid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of FamilyPlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadFamiliesInArea = function (areaid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddFamiliesToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadFamiliesInArea", { areaid: areaid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of SmallGroupPlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadGroupsInArea = function (areaid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddGroupsToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadGroupsInArea", { areaid: areaid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadPeopleInProfile = function (profileid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddPeopleToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadPeopleInProfile", { profileid: profileid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadPeopleInReport = function (reportid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddPeopleToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadPeopleInReport", { reportid: reportid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadPeopleInCategory = function (categoryid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddPeopleToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadPeopleInCategory", { categoryid: categoryid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadGroupsInCategory = function (categoryid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddGroupsToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadGroupsInCategory", { categoryid: categoryid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadPeopleInCluster = function (clusterid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddPeopleToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadPeopleInCluster", { clusterid: clusterid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadGroupsInCluster = function (clusterid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddGroupsToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadGroupsInCluster", { clusterid: clusterid }, callback, 0);
    };


    //
    // callback(error, data, finished)
    // error is a true/false indicating if an error occurred.
    // data is an array of PeoplePlacemark objects.
    // finished is a true/false indicating if any more data will be returned.
    //
    this.LoadPeopleInGroup = function (groupid, callback) {
        if (typeof (callback) == 'undefined' || callback == null)
            callback = this._AddPeopleToMap;

        var g = this;
        g._StartWork();
        g._LoadInCommon("LoadPeopleInGroup", { groupid: groupid }, callback, 0);
    };


    //
    // Register a map marker to receive the standard click
    // handler for person details.
    //
    this.RegisterPersonInfoPopup = function (marker) {
        marker.googlemap = this;
        google.maps.event.addListener(marker, 'click', this._ShowStandardDetailsInfoPopup);
    };


    //
    // Register a map marker to receive the standard click
    // handler for family details.
    //
    this.RegisterFamilyInfoPopup = function (marker) {
        marker.googlemap = this;
        google.maps.event.addListener(marker, 'click', this._ShowStandardDetailsInfoPopup);
    };


    //
    // Register a map marker to receive the standard click
    // handler for small group details.
    //
    this.RegisterGroupInfoPopup = function (marker) {
        marker.googlemap = this;
        google.maps.event.addListener(marker, 'click', this._ShowStandardDetailsInfoPopup);
    };


    //
    // Internal method to handle loading in the background.
    // The cmd is the type of loading to perform as this method can
    // deal with multiple types, such as individuals, families and
    // small groups.
    //
    this._LoadInCommon = function (cmd, args, callback, offset) {
        var g = this;
        args.start = offset;
        args.count = 100;
        $.ajax({
            url: this.serviceurl + "/" + cmd,
            data: JSON.stringify(args),
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            success: function (data) {
                callback(false, data.d, (data.d.length < 100), g);
                if (data.d.length == 100)
                    g._LoadInCommon(cmd, args, callback, offset + 100);
                else
                    g._StopWork();
            },
            error: function () {
                callback(true, new Array(), false, g);
                g._StopWork();
            }
        });
    };


    //
    // Add the objects specified to the map.
    //
    this._AddPeopleToMap = function (error, people, finished, g) {
        if (error == false) {
            for (i = 0; i < people.length; i++) {
                var marker = new PersonMarker({
                    icon: people[i].PinImage,
                    position: new google.maps.LatLng(people[i].Latitude, people[i].Longitude),
                    map: g.map,
                    title: people[i].Name
                }, people[i].Unique);
                g.RegisterPersonInfoPopup(marker);
            }
        }
    };


    //
    // Add the objects specified to the map.
    //
    this._AddFamiliesToMap = function (error, families, finished, g) {
        if (error == false) {
            for (i = 0; i < families.length; i++) {
                var marker = new FamilyMarker({
                    icon: families[i].PinImage,
                    position: new google.maps.LatLng(families[i].Latitude, families[i].Longitude),
                    map: g.map,
                    title: families[i].Name
                }, families[i].Unique);
                g.RegisterFamilyInfoPopup(marker);
            }
        }
    };


    //
    // Add the objects specified to the map.
    //
    this._AddGroupsToMap = function (error, groups, finished, g) {
        if (error == false) {
            for (i = 0; i < groups.length; i++) {
                var marker = new GroupMarker({
                    icon: groups[i].PinImage,
                    position: new google.maps.LatLng(groups[i].Latitude, groups[i].Longitude),
                    map: g.map,
                    title: groups[i].Name
                }, groups[i].Unique);
                g.RegisterGroupInfoPopup(marker);
            }
        }
    };


    //
    // Show the person details popup for the selected marker.
    //
    this._ShowStandardDetailsInfoPopup = function (event) {
        var g = this.googlemap;
        var cmd = "";
        var data = {};
        var marker = this;

        if (g.infowindow != null)
            g.infowindow.close();
        g.infowindow = new google.maps.InfoWindow({ content: '<div style="text-align: center"><img src="' + GoogleMapRoot + 'Images/ajax-spin.gif" style="border: none;" /></div>', maxWidth: 350 });
        g.infowindow.open(marker.map, marker);

        if (typeof (this.personID) != 'undefined') {
            cmd = "PersonDetailsInfoWindow";
            data.personID = this.personID;
        }
        else if (typeof (this.familyID) != 'undefined') {
            cmd = "FamilyDetailsInfoWindow";
            data.familyID = this.familyID;
        }
        else if (typeof (this.groupID) != 'undefined') {
            cmd = "GroupDetailsInfoWindow";
            data.groupID = this.groupID;
        }
        else
            alert('Unknown marker type.');

        $.ajax({
            url: g.serviceurl + "/" + cmd,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            dataType: "json",
            success: function (data, status) {
                g.infowindow.close();
                g.infowindow.setContent(data.d);
                g.infowindow.open(marker.map, marker);
            },
            error: function (a, b, c) {
                g.infowindow.setContent('Failed to load details.');
            }
        });
    }


    //
    // Show that we have started some background processing.
    //
    this._StartWork = function () {
        if (this.workingCount == 0)
            this.workingDiv.show();

        this.workingCount += 1;
    };


    //
    // Show that we have finished background processing.
    //
    this._StopWork = function () {
        this.workingCount -= 1;

        if (this.workingCount == 0)
            this.workingDiv.hide();
    };
};


