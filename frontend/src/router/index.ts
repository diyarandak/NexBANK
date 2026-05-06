import { createRouter, createWebHistory } from 'vue-router';
import LoginView from '../views/LoginView.vue';
import DashboardView from '../views/DashboardView.vue';
import AccountsView from '../views/AccountsView.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView, meta: { requiresAuth: false } },
    { path: '/login-staff', name: 'login-staff', component: () => import('../views/LoginStaffView.vue'), meta: { requiresAuth: false } },
    { path: '/register', name: 'register', component: () => import('../views/RegisterView.vue'), meta: { requiresAuth: false } },
    { path: '/', name: 'intro', component: () => import('../views/IntroView.vue'), meta: { requiresAuth: false } },
    
    // 👤 MÜŞTERİ ROTALARI
    { path: '/dashboard', name: 'dashboard', component: DashboardView, meta: { requiresAuth: true } },
    { path: '/accounts', name: 'accounts', component: AccountsView, meta: { requiresAuth: true } },
    { path: '/cards', name: 'cards', component: () => import('../views/CardsView.vue'), meta: { requiresAuth: true } },
    { path: '/cash-advance', name: 'cash-advance', component: () => import('../views/CashAdvanceView.vue'), meta: { requiresAuth: true } },
    { path: '/credit-card-payment', name: 'credit-card-payment', component: () => import('../views/CreditCardPaymentView.vue'), meta: { requiresAuth: true } },
    { path: '/wallet', name: 'wallet', component: () => import('../views/WalletView.vue'), meta: { requiresAuth: true } },
    
    { path: '/transfer', name: 'transfer', component: () => import('../views/TransferView.vue'), meta: { requiresAuth: true } },
    { path: '/qr-pay', name: 'qr-pay', component: () => import('../views/QRPayView.vue'), meta: { requiresAuth: true } },
    { path: '/payments', name: 'payments', component: () => import('../views/PaymentsView.vue'), meta: { requiresAuth: true } },
    { path: '/scheduled-payments', name: 'scheduled-payments', component: () => import('../views/ScheduledPaymentsView.vue'), meta: { requiresAuth: true } },
    { path: '/transactions', name: 'transactions', component: () => import('../views/TransactionsView.vue'), meta: { requiresAuth: true } },
    
    { path: '/loans', name: 'loans', component: () => import('../views/LoansView.vue'), meta: { requiresAuth: true } },
    { path: '/my-applications', name: 'my-applications', component: () => import('../views/MyApplicationsView.vue'), meta: { requiresAuth: true } },
    { path: '/limit-request', name: 'limit-request', component: () => import('../views/LimitRequestView.vue'), meta: { requiresAuth: true } },
    
    { path: '/stocks', name: 'stocks', component: () => import('../views/StocksView.vue'), meta: { requiresAuth: true } },
    { path: '/exchange', name: 'exchange', component: () => import('../views/ExchangeView.vue'), meta: { requiresAuth: true } },
    { path: '/funds', name: 'funds', component: () => import('../views/FundsView.vue'), meta: { requiresAuth: true } },
    { path: '/portfolio', name: 'portfolio', component: () => import('../views/PortfolioView.vue'), meta: { requiresAuth: true } },
    
    { path: '/notifications', name: 'notifications', component: () => import('../views/NotificationsView.vue'), meta: { requiresAuth: true } },
    { path: '/documents', name: 'documents', component: () => import('../views/DocumentsView.vue'), meta: { requiresAuth: true } },
    { path: '/settings', name: 'settings', component: () => import('../views/SettingsView.vue'), meta: { requiresAuth: true } },
    { path: '/support', name: 'support', component: () => import('../views/SupportView.vue'), meta: { requiresAuth: true } },
    { path: '/rewards', name: 'rewards', component: () => import('../views/RewardCenterView.vue'), meta: { requiresAuth: true } },
    { path: '/market', name: 'market', component: () => import('../views/MarketplaceView.vue'), meta: { requiresAuth: true } },


    // 🏢 PERSONEL ROTALARI
    { path: '/approval-center', name: 'approval-center', component: () => import('../views/ApprovalCenterView.vue'), meta: { requiresAuth: true } },
    { path: '/staff-transactions', name: 'staff-transactions', component: () => import('../views/StaffTransactionsView.vue'), meta: { requiresAuth: true } },
    { path: '/fraud-alerts', name: 'fraud-alerts', component: () => import('../views/FraudAlertsView.vue'), meta: { requiresAuth: true } },
    
    { path: '/customers', name: 'customers', component: () => import('../views/CustomersView.vue'), meta: { requiresAuth: true } },
    { path: '/kyc-verification', name: 'kyc-verification', component: () => import('../views/KycVerificationView.vue'), meta: { requiresAuth: true } },
    { path: '/staff-loans', name: 'staff-loans', component: () => import('../views/StaffLoansView.vue'), meta: { requiresAuth: true } },
    
    { path: '/daily-report', name: 'daily-report', component: () => import('../views/ReportsView.vue'), meta: { requiresAuth: true } },
    { path: '/analytics', name: 'analytics', component: () => import('../views/ReportsView.vue'), meta: { requiresAuth: true } },
    { path: '/reconciliation', name: 'reconciliation', component: () => import('../views/ReconciliationView.vue'), meta: { requiresAuth: true } },
    
    { path: '/audit-logs', name: 'audit-logs', component: () => import('../views/AuditLogsView.vue'), meta: { requiresAuth: true } },
    { path: '/staff-management', name: 'staff-management', component: () => import('../views/StaffManagementView.vue'), meta: { requiresAuth: true } },
    { path: '/system-logs', name: 'system-logs', component: () => import('../views/SystemLogsView.vue'), meta: { requiresAuth: true } },
    
    { path: '/bank-settings', name: 'bank-settings', component: () => import('../views/BankSettingsView.vue'), meta: { requiresAuth: true } },
    { path: '/campaign-management', name: 'campaign-management', component: () => import('../views/CampaignManagementView.vue'), meta: { requiresAuth: true } },
    { path: '/broadcast', name: 'broadcast', component: () => import('../views/BroadcastView.vue'), meta: { requiresAuth: true } }
  ]
});

// Auth Guard
router.beforeEach((to, _, next) => {
  const token = localStorage.getItem('token');
  if (to.meta.requiresAuth && !token) {
    next({ name: 'login' });
  } else {
    next();
  }
});

export default router;
