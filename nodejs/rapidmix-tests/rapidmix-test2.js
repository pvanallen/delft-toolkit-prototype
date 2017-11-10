var osc = require('node-osc');
var rapidMix = require('rapidlib');

// set up RapidMix classification
//
// Create a series classificationm object
var myDTW = new rapidMix.SeriesClassification();
// these arrays will hold two training sets and one to test against.
var testSet = [];
var seriesSet = [];

// set up for reading the data file
var lines = [];
var lineReader = require('readline').createInterface({
  input: require('fs').createReadStream('data.csv')
});
lineReader.on('line', function (line) {
  var lineArr = line.split(",");
  lines.push(lineArr);
});

// once the file is loaded, process the data
lineReader.on('close', function (line) {
  console.log("all lines in file read");

  // go through the data lines
  for (var i=0; i<lines.length; i+=3) {
    // set the category for data
    var gestureTrainingSet = { input: [], label: lines[i][1] };
    // get the x,y,z data from 3 consecutive lines
    for (var n=4; n<lines[i].length; n++) {
      var x = parseFloat(lines[i][n]);
      var y = parseFloat(lines[i+1][n]);
      var z = parseFloat(lines[i+2][n]);
      var rapidInput = [x,y,z,0.0];
      gestureTrainingSet.input.push(rapidInput);
    }

    // add a gesture dataset to series
    console.log("trainingset " + lines[i][1] + ": " + gestureTrainingSet.input.length);
    seriesSet.push(gestureTrainingSet);
  }
  // build test set
  for (var i=4; i<lines[0].length; i++) {
  //for (var i=4; i<132; i++) {
    // build test case
    var start = 30;
    var x = 0.0;
    var y = 0.0;
    var z = 0.0;
    if (i<lines[start].length) {
      x = parseFloat(lines[start][i]);
      y = parseFloat(lines[start+1][i]);
      z = parseFloat(lines[start+2][i]);
    } // else pad with zeros
    var rapidInput = [x,y,z,0.0];
    testSet.push(rapidInput);
  }

  console.log(seriesSet[0].input);
  console.log(seriesSet[8].input);
  console.log(testSet);
  trainEvaluate();
});

// train model and evaluate case
function trainEvaluate() {
  myDTW.reset();
  console.log("train");
  console.log(myDTW.train(seriesSet));
  console.log("run");
  match = myDTW.run(testSet);
  console.log("getCosts");
  costs = myDTW.getCosts();
  //console.log("testSet ", testSet);
  console.log("match ", match);
  console.log("costs ", costs);
};
