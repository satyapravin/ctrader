import React, { Component } from 'react';

class DateFormatter extends Component {
  render() {
    // Unixtimestamp
    var unixtimestamp = this.props.value;
    if((unixtimestamp == null || unixtimestamp.length===0) && this.props.colDef.field ==="last_updated") {
      unixtimestamp = Math.floor(Date.now() / 1000);
    }
    // Months array
    //var months_arr = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    // Convert timestamp to milliseconds
    var date = new Date(unixtimestamp * 1000);
    // Year
    var year = date.getFullYear();
    // Month
    //var month = months_arr[date.getMonth()];
    var month = date.getMonth()+1;
    // Day
    var day = date.getDate();
    // Hours
    var hours = date.getHours();
    // Minutes
    var minutes = "0" + date.getMinutes();
    // Seconds
    var seconds = "0" + date.getSeconds();
    // Display date time in MM-dd-yyyy h:m:s format
    //var convdataTime = month + '-' + day + '-' + year + ' ' + hours + ':' + minutes.substr(-2) + ':' + seconds.substr(-2);
    var convdataTime ='';
    if(unixtimestamp)
      convdataTime = year + '-' +month + '-' + day + ' ' +  hours + ':' + minutes.substr(-2) + ':' + seconds.substr(-2);
 
    return this.showPage(convdataTime);
  }
  showPage(date) {
    return (
      <div>{date}</div>
    );
  }
}

export default DateFormatter;
