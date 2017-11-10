var osc = require('node-osc');
var rapidMix = require('rapidlib');

// set up RapidMix classification
//
// Create a series classificationm object
var myDTW = new rapidMix.SeriesClassification();
// these arrays will hold two training sets and one to test against.
var gesture1TrainingSet = { input: [], label: "gesture1" };
var gesture2TrainingSet = { input: [], label: "gesture2" };
var testSet = [];

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
  console.log("all done");
  for (var i=4; i<lines[0].length - 42; i++) {
    // build gesture1
    var x = parseFloat(lines[0][i]);
    var y = parseFloat(lines[1][i]);
    var z = parseFloat(lines[2][i]);
    var rapidInput = [x,y,z,0.0];
    recorder(1, rapidInput);
    // build gesture2
    var x = parseFloat(lines[24][i]);
    var y = parseFloat(lines[25][i]);
    var z = parseFloat(lines[26][i]);
    var rapidInput = [x,y,z,0.0];
    recorder(2, rapidInput);
    // build test case
    var x = parseFloat(lines[3][i]);
    var y = parseFloat(lines[4][i]);
    var z = parseFloat(lines[5][i]);
    var rapidInput = [x,y,z,0.0];
    recorder(0, rapidInput);
  }
  console.log(gesture1TrainingSet);
  console.log(gesture2TrainingSet);
  console.log(testSet);
  trainEvaluate();
});

//this pushes the data into the correct set
function recorder(dataMode, rapidInput) {
  switch (dataMode) {
    case 1: // add to gesture 1 training set
      gesture1TrainingSet.input.push(rapidInput);
      break;
    case 2: // add to gesture 2 training set
      gesture2TrainingSet.input.push(rapidInput);
      break;
    case 0: // add to classification set
      testSet.push(rapidInput);
      break;
  }
}

// train model and evaluate case
function trainEvaluate() {
  console.log(testSet.length);
  recordState = 0;
  myDTW.reset();
  var seriesSet = [gesture1TrainingSet, gesture2TrainingSet];
  console.log("train");
  console.log(myDTW.train(seriesSet));
  console.log("run");
  match = myDTW.run(testSet);
  console.log("getCosts");
  costs = myDTW.getCosts();
  console.log("testSet ", testSet);
  console.log("match ", match);
  console.log("costs ", costs);
};
