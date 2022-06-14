import React from 'react';
import ReactDOM from 'react-dom';
import Menu from "./jsx/menu";
import './bootstrap.css';
import './react.css';

console.log('start');
ReactDOM.render(
    <React.StrictMode>
        <Menu />
    </React.StrictMode>
  ,
  document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
//reportWebVitals();

//ReactDOM.render(
//    <React.StrictMode>
//        <Test />
//    </React.StrictMode>
//    ,
//    document.getElementById('root')
//);

// visibilityCss={[]} visibilityFlag={[]} menu={[]}