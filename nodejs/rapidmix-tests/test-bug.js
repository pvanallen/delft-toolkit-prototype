var rapidMix = require('rapidlib');
var testDTW = new rapidMix.SeriesClassification();

testSet2 = [];
for (let i = 0; i < 5; ++i) {
    testSet2.push([0.1, 0.1, 0.1, 0.1]);
}

let series2 = {input: testSet2, label: "yyy"};
let series1 = {input: testSet2, label: "zzz"};
let sset = [series1, series2];

console.log(testDTW.train(sset));
console.log(testDTW.run(testSet2));
