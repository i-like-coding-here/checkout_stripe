import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css'
import CheckoutSuccess from './CheckoutSuccess';
import CheckoutCancelled from './CheckoutCancelled';
// import PlansPage from './NewPlan';
import NewPlan from './NewPlan';
import ActionPlan from './ActionPlan';
import UpdatePlans from './UpdatePlan';

function App() {
  return (
    <>
      <Router>
        <Routes>
          <Route path="/" element={<ActionPlan />} />
          <Route path='/newplan' element={<NewPlan/>}/>
          <Route path='/updateplan' element={<UpdatePlans/>}/>
          <Route path="/success" element={<CheckoutSuccess />} />
          <Route path="/canceled" element={<CheckoutCancelled />} />
        </Routes>
      </Router>
    </>
  )
}

export default App
