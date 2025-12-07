<template>
  <div class="coffee-machine">
    <header class="header">
      <h1> Máquina Expendedora de Café</h1>
      <p class="subtitle">Selecciona tus cafés y realiza el pago</p>
    </header>

    <div class="container">
      <!-- Cafés Disponibles -->
      <section class="section coffees-section">
        <h2>Cafés Disponibles</h2>
        <div class="coffee-list">
          <div 
            v-for="coffee in coffees" 
            :key="coffee.name"
            class="coffee-item"
            :class="{ 'out-of-stock': coffee.quantity === 0 }"
          >
            <div class="coffee-info">
              <h3>{{ coffee.name }}</h3>
              <p class="price">₡{{ coffee.price }}</p>
              <p class="quantity" :class="{ 'low-stock': coffee.quantity <= 3 }">
                Disponibles: {{ coffee.quantity }}
              </p>
            </div>
            
            <div class="coffee-controls">
              <button 
                @click="decreaseCoffee(coffee.name)"
                :disabled="order[coffee.name] <= 0"
                class="btn-quantity"
              >
                -
              </button>
              
              <input
                type="number"
                v-model.number="order[coffee.name]"
                :min="0"
                :max="coffee.quantity"
                @input="validateCoffeeQuantity(coffee.name, $event)"
                class="quantity-input"
                :disabled="coffee.quantity === 0"
              />
              
              <button 
                @click="increaseCoffee(coffee.name)"
                :disabled="coffee.quantity === 0 || order[coffee.name] >= coffee.quantity"
                class="btn-quantity"
              >
                +
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Pago -->
      <section class="section payment-section">
        <h2>Ingresar Pago</h2>
        
        <!-- Monedas -->
        <div class="coins-section">
          <h3>Monedas</h3>
          <div class="coins-grid">
            <div 
              v-for="coin in [500, 100, 50, 25]" 
              :key="coin"
              class="coin-item"
            >
              <span class="coin-label">₡{{ coin }}</span>
              <div class="coin-controls">
                <button @click="decreaseCoin(coin)" :disabled="payment.coins[coin] <= 0" class="btn-small">-</button>
                <input 
                  type="number" 
                  v-model.number="payment.coins[coin]"
                  min="0"
                  class="coin-input"
                />
                <button @click="increaseCoin(coin)" class="btn-small">+</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Billetes -->
        <div class="bills-section">
          <h3>Billetes</h3>
          <div class="bill-item">
            <span class="bill-label">₡1000</span>
            <div class="bill-controls">
              <button @click="decreaseBill(1000)" :disabled="payment.bills[1000] <= 0" class="btn-small">-</button>
              <input 
                type="number" 
                v-model.number="payment.bills[1000]"
                min="0"
                class="coin-input"
              />
              <button @click="increaseBill(1000)" class="btn-small">+</button>
            </div>
          </div>
        </div>

        <!-- Resumen de Pago -->
        <div class="payment-summary">
          <h3>Resumen de Pago</h3>
          <div class="summary-item">
            <span>Total seleccionado:</span>
            <span class="amount">₡{{ totalSelected }}</span>
          </div>
          <div class="summary-item">
            <span>Total ingresado:</span>
            <span class="amount">₡{{ totalPayment }}</span>
          </div>
          <div class="summary-item" v-if="totalPayment > 0">
            <span>Cambio necesario:</span>
            <span class="amount" :class="{ 'insufficient': totalPayment < totalSelected }">
              ₡{{ changeNeeded }}
            </span>
          </div>
        </div>
      </section>

      <!-- Acciones y Resultados -->
      <section class="section actions-section">
        <h2>Acciones</h2>
        
        <div class="action-buttons">
          <button 
            @click="calculateTotal" 
            :disabled="Object.keys(order).length === 0"
            class="btn btn-primary"
          >
            Calcular Total (₡{{ totalSelected }})
          </button>
          
          <button 
            @click="buyCoffee" 
            :disabled="!canBuy || isProcessing"
            class="btn btn-success"
          >
            {{ isProcessing ? 'Procesando...' : 'Comprar' }}
          </button>
          
          <button @click="resetAll" class="btn btn-secondary">
            Reiniciar
          </button>
        </div>

        <!-- Resultados -->
        <div v-if="result" class="result-container">
          <div v-if="result.success" class="result-success">
            <h3>Compra Exitosa</h3>
            <p>{{ result.message }}</p>
            
            <div v-if="result.change && Object.keys(result.change).length > 0" class="change-breakdown">
              <h4>Desglose del cambio:</h4>
              <ul>
                <li v-for="(count, denomination) in result.change" :key="denomination">
                  {{ count }} moneda{{ count > 1 ? 's' : '' }} de ₡{{ denomination }}
                </li>
              </ul>
            </div>
            
            <div class="updated-inventory">
              <h4>Cafés Disponibles:</h4>
              <div class="inventory-grid">
                <div v-for="coffee in result.updatedCoffees" :key="coffee.name" class="inventory-item">
                  <span>{{ coffee.name }}:</span>
                  <span>{{ coffee.quantity }} disponibles</span>
                </div>
              </div>
            </div>
          </div>
          
          <div v-else class="result-error">
            <h3>Error</h3>
            <p>{{ result.message }}</p>
          </div>
        </div>

        <!-- Estado de la máquina -->
        <div class="machine-status">
          <h3>Estado de la Máquina</h3>
          <p v-if="machineStatus.isOperational" class="status-ok">
            Máquina operativa
          </p>
          <p v-else class="status-error">
            Máquina sin cambio disponible
          </p>
          
          <div class="coins-status">
            <h4>Monedas disponibles para cambio:</h4>
            <div class="coins-grid-small">
              <span v-for="coin in machineStatus.coins" :key="coin.denomination" 
                    class="coin-status" :class="{ 'low-coin': coin.quantity <= 5 }">
                ₡{{ coin.denomination }}: {{ coin.quantity }}
              </span>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import apiConfig from './apiConfig';

export default {
  name: 'CoffeeMachine',
  data() {
    return {
      apiConfig: apiConfig,
      coffees: [],
      order: {},
      payment: {
        coins: { 500: 0, 100: 0, 50: 0, 25: 0 },
        bills: { 1000: 0 }
      },
      result: null,
      isProcessing: false,
      machineStatus: {
        isOperational: true,
        coins: []
      }
    };
  },
  
  computed: {
    totalSelected() {
      let total = 0;
      for (const [coffeeName, quantity] of Object.entries(this.order)) {
        const coffee = this.coffees.find(c => c.name === coffeeName);
        if (coffee && quantity > 0) {
          total += coffee.price * quantity;
        }
      }
      return total;
    },
    
    totalPayment() {
      let total = 0;
      
      for (const [denomination, count] of Object.entries(this.payment.coins)) {
        total += parseInt(denomination) * count;
      }
      
      for (const [denomination, count] of Object.entries(this.payment.bills)) {
        total += parseInt(denomination) * count;
      }
      
      return total;
    },
    
    changeNeeded() {
      return this.totalPayment - this.totalSelected;
    },
    
    canBuy() {
      return this.totalSelected > 0 && 
             this.totalPayment >= this.totalSelected &&
             !this.isProcessing;
    }
  },
  
  async created() {
    await this.loadInitialData();
  },
  
  methods: {
    async loadInitialData() {
        try {
            const [coffeesResponse, coinsResponse] = await Promise.all([
              axios.get(this.apiConfig.getCoffeeMachineUrl('/coffees')),
              axios.get(this.apiConfig.getCoffeeMachineUrl('/coins'))
            ]);
            
            this.coffees = coffeesResponse.data;
            this.machineStatus.coins = coinsResponse.data.filter(coin => coin.denomination !== 1000);
            this.machineStatus.isOperational = this.machineStatus.coins.some(coin => coin.quantity > 0);
            
            const newOrder = {};
            this.coffees.forEach(coffee => {
            newOrder[coffee.name] = 0;
            });
            this.order = newOrder; 
            
        } catch (error) {
            console.error('Error cargando datos iniciales:', error);
            alert('Error conectando con la máquina de café');
        }
        },
    
    increaseCoffee(coffeeName) {
      const coffee = this.coffees.find(c => c.name === coffeeName);
      if (coffee && this.order[coffeeName] < coffee.quantity) {
        this.order[coffeeName]++;
      }
    },
    
    decreaseCoffee(coffeeName) {
      if (this.order[coffeeName] > 0) {
        this.order[coffeeName]--;
      }
    },
    
    validateCoffeeQuantity(coffeeName, event) {
      const coffee = this.coffees.find(c => c.name === coffeeName);
      if (coffee) {
        const value = parseInt(event.target.value) || 0;
        this.order[coffeeName] = Math.min(Math.max(value, 0), coffee.quantity);
      }
    },
    
    increaseCoin(denomination) {
      this.payment.coins[denomination]++;
    },
    
    decreaseCoin(denomination) {
      if (this.payment.coins[denomination] > 0) {
        this.payment.coins[denomination]--;
      }
    },
    
    increaseBill(denomination) {
      this.payment.bills[denomination]++;
    },
    
    decreaseBill(denomination) {
      if (this.payment.bills[denomination] > 0) {
        this.payment.bills[denomination]--;
      }
    },
    
    async calculateTotal() {
      try {
        const orderToSend = {};
        for (const [coffeeName, quantity] of Object.entries(this.order)) {
          if (quantity > 0) {
            orderToSend[coffeeName] = quantity;
          }
        }
        
        if (Object.keys(orderToSend).length === 0) {
          alert('Selecciona al menos un café');
          return;
        }
        
        const response = await axios.post(
          this.apiConfig.getCoffeeMachineUrl('/calculate-total'), 
          orderToSend
        );
        alert(`Total de la compra: ₡${response.data}`);
        
      } catch (error) {
        console.error('Error calculando total:', error);
        alert(error.response?.data || 'Error calculando el total');
      }
    },
    
    async buyCoffee() {
      this.isProcessing = true;
      this.result = null;
      
      try {
        const orderToSend = {};
        for (const [coffeeName, quantity] of Object.entries(this.order)) {
          if (quantity > 0) {
            orderToSend[coffeeName] = quantity;
          }
        }
        
        const coinsToSend = [];
        for (const [denomination, count] of Object.entries(this.payment.coins)) {
          for (let i = 0; i < count; i++) {
            coinsToSend.push(parseInt(denomination));
          }
        }
        
        const billsToSend = [];
        for (const [denomination, count] of Object.entries(this.payment.bills)) {
          for (let i = 0; i < count; i++) {
            billsToSend.push(parseInt(denomination));
          }
        }
        
        const request = {
          order: orderToSend,
          payment: {
            totalAmount: this.totalPayment,
            coins: coinsToSend,
            bills: billsToSend
          }
        };
        
        const response = await axios.post(
          this.apiConfig.getCoffeeMachineUrl('/buy'), 
          request
        );
        this.result = response.data;
        
        if (this.result.success) {
          this.coffees = this.result.updatedCoffees;
          await this.loadInitialData(); 
        }
         
        this.resetPayment();
        
      } catch (error) {
        console.error('Error comprando:', error);
        this.result = {
          success: false,
          message: error.response?.data?.error || 'Error procesando la compra'
        };
      } finally {
        this.isProcessing = false;
      }
    },

    resetPayment() {
      Object.keys(this.payment.coins).forEach(denomination => {
        this.payment.coins[denomination] = 0;
      });
      Object.keys(this.payment.bills).forEach(denomination => {
        this.payment.bills[denomination] = 0;
      });
    },
    
    resetAll() {
      for (const coffeeName in this.order) {
        this.order[coffeeName] = 0;
      }
      
      this.payment.coins = { 500: 0, 100: 0, 50: 0, 25: 0 };
      this.payment.bills = { 1000: 0 };
      
      this.result = null;
    }
  }
};
</script>

<style scoped>
.coffee-machine {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.header {
  text-align: center;
  margin-bottom: 30px;
  padding: 20px;
  background: linear-gradient(135deg, #6f4e37 0%, #4a3526 100%);
  color: white;
  border-radius: 10px;
}

.header h1 {
  margin: 0;
  font-size: 2.5rem;
}

.subtitle {
  margin: 10px 0 0;
  opacity: 0.9;
}

.container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 30px;
}

@media (max-width: 1024px) {
  .container {
    grid-template-columns: 1fr;
  }
}

.section {
  background: white;
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.section h2 {
  margin-top: 0;
  color: #6f4e37;
  border-bottom: 2px solid #6f4e37;
  padding-bottom: 10px;
}

.coffee-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.coffee-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  transition: all 0.3s;
}

.coffee-item:hover {
  border-color: #6f4e37;
}

.coffee-item.out-of-stock {
  opacity: 0.5;
  background: #f5f5f5;
}

.coffee-info h3 {
  margin: 0 0 5px;
  color: #333;
}

.price {
  font-size: 1.2rem;
  font-weight: bold;
  color: #2e7d32;
  margin: 5px 0;
}

.quantity {
  font-size: 0.9rem;
  color: #666;
}

.quantity.low-stock {
  color: #f57c00;
  font-weight: bold;
}

.coffee-controls {
  display: flex;
  gap: 10px;
  align-items: center;
}

.btn-quantity {
  width: 40px;
  height: 40px;
  border: none;
  background: #6f4e37;
  color: white;
  border-radius: 50%;
  font-size: 1.2rem;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-quantity:hover:not(:disabled) {
  background: #4a3526;
}

.btn-quantity:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.quantity-input {
  width: 60px;
  padding: 8px;
  text-align: center;
  border: 2px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.quantity-input:disabled {
  background: #f5f5f5;
}

/* Pago */
.coins-section, .bills-section {
  margin-bottom: 20px;
}

.coins-section h3, .bills-section h3 {
  color: #555;
  margin-bottom: 15px;
}

.coins-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 15px;
}

.coin-item, .bill-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px;
  background: #f9f9f9;
  border-radius: 6px;
}

.coin-label, .bill-label {
  font-weight: bold;
  color: #333;
}

.coin-controls, .bill-controls {
  display: flex;
  gap: 8px;
  align-items: center;
}

.btn-small {
  width: 30px;
  height: 30px;
  border: none;
  background: #4a3526;
  color: white;
  border-radius: 4px;
  cursor: pointer;
}

.btn-small:hover:not(:disabled) {
  background: #6f4e37;
}

.btn-small:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.coin-input {
  width: 50px;
  padding: 5px;
  text-align: center;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.payment-summary {
  margin-top: 20px;
  padding: 15px;
  background: #f0f7ff;
  border-radius: 8px;
  border-left: 4px solid #2196f3;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  margin: 8px 0;
  font-size: 1.1rem;
}

.amount {
  font-weight: bold;
}

.amount.insufficient {
  color: #f44336;
}

.actions-section {
  grid-column: 1 / -1;
}

.action-buttons {
  display: flex;
  gap: 15px;
  margin-bottom: 30px;
  flex-wrap: wrap;
}

.btn {
  padding: 12px 24px;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.3s;
  flex: 1;
  min-width: 150px;
}

.btn-primary {
  background: #2196f3;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0b7dda;
}

.btn-success {
  background: #4caf50;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #388e3c;
}

.btn-secondary {
  background: #757575;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #616161;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.result-container {
  margin-top: 30px;
  padding: 20px;
  border-radius: 8px;
  animation: slideIn 0.5s ease;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.result-success {
  background: #e8f5e9;
  border-left: 4px solid #4caf50;
  padding: 20px;
}

.result-error {
  background: #ffebee;
  border-left: 4px solid #f44336;
  padding: 20px;
}

.change-breakdown {
  margin: 15px 0;
  padding: 15px;
  background: white;
  border-radius: 6px;
}

.change-breakdown ul {
  list-style: none;
  padding: 0;
  margin: 10px 0 0;
}

.change-breakdown li {
  padding: 5px 0;
  border-bottom: 1px solid #eee;
}

.change-breakdown li:last-child {
  border-bottom: none;
}

.updated-inventory {
  margin-top: 20px;
}

.inventory-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 10px;
  margin-top: 10px;
}

.inventory-item {
  display: flex;
  justify-content: space-between;
  padding: 8px;
  background: white;
  border-radius: 4px;
}

.machine-status {
  margin-top: 30px;
  padding: 20px;
  background: #f5f5f5;
  border-radius: 8px;
}

.status-ok {
  color: #4caf50;
  font-weight: bold;
}

.status-error {
  color: #f57c00;
  font-weight: bold;
}

.coins-status {
  margin-top: 15px;
}

.coins-grid-small {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 10px;
}

.coin-status {
  padding: 5px 10px;
  background: white;
  border-radius: 4px;
  font-size: 0.9rem;
}

.coin-status.low-coin {
  background: #fff3e0;
  color: #f57c00;
  font-weight: bold;
}
</style>