const apiConfig = {
  baseURL: 'https://localhost:7183',
  
  getCoffeeMachineUrl(endpoint) {
    return `${this.baseURL}/api/CoffeeMachine${endpoint}`;
  }
};

export default apiConfig;