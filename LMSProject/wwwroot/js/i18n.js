// ══════════════════════════════════════════════════════════════════════
// TOTC LMS — Frontend Language Toggle (Arabic ↔ English)
// Strategy: data-i18n attributes (primary) + text-node fallback
// ══════════════════════════════════════════════════════════════════════

var TRANSLATIONS = {
    "Home": "الرئيسية", "Blog": "المدونة", "About": "من نحن",
    "Login": "تسجيل الدخول", "Logout": "تسجيل الخروج",
    "Dashboard": "لوحة التحكم", "العربية": "English",

    // HOME
    "Studying Online is now much easier": "التعلم عبر الإنترنت أصبح أسهل بكثير",
    "TOTC is an interesting platform that will teach you in more interactive way": "TOTC منصة تعليمية تفاعلية تجعل التعلم أكثر متعة وفاعلية",
    "Join for free": "انضم مجانًا", "Watch how it works": "شاهد كيف تعمل",
    "Our Success": "إنجازاتنا",
    "Ornare id fames interdum porttitor nulla turpis etiam. Diam vitae sollicitudin at nec nam et pharetra gravida.": "منصة TOTC تحقق نتائج حقيقية وملموسة لآلاف الطلاب.",
    "Students": "طلاب", "Total success": "نسبة النجاح", "Main questions": "أسئلة رئيسية",
    "Chief experts": "خبراء", "Years of experience": "سنوات خبرة",
    "All-In-One Cloud Software": "برنامج متكامل في السحابة",
    "TOTC is one powerful online software suite that combines all tools": "TOTC هو برنامج متكامل يجمع كل الأدوات التعليمية",
    "Online Billing, Invoicing, & Contracts": "الفوترة والعقود الإلكترونية",
    "Simple and secure control of your organization's financial and legal transactions.": "إدارة آمنة للمعاملات المالية والقانونية.",
    "Easy Scheduling & Attendance Tracking": "الجدولة وتتبع الحضور",
    "Schedule and reserve classrooms and track student attendance easily.": "جدول قاعات الدراسة وتتبع حضور الطلاب بسهولة.",
    "Customer Tracking": "تتبع المستخدمين",
    "Automate and track emails to individuals or groups and manage engagement.": "أتمتة وتتبع الرسائل وإدارة التواصل.",
    "What is TOTC?": "ما هو TOTC؟",
    "TOTC is a platform that allows educators and learners to connect, collaborate, and grow together in a modern learning environment.": "TOTC منصة تتيح للمعلمين والمتعلمين التواصل والتعاون والنمو معاً.",
    "FOR INSTRUCTORS": "للمدرسين", "FOR STUDENTS": "للطلاب",
    "Start a class today": "ابدأ فصلك الآن", "Enter access code": "أدخل رمز الوصول",
    "Everything you can do in a physical classroom": "كل ما يمكنك فعله في الفصل الدراسي التقليدي",
    "TOTC allows you to replicate and enhance classroom experiences online with powerful tools.": "TOTC يتيح محاكاة وتطوير تجربة الفصل الدراسي عبر الإنترنت.",
    "Interactive lessons": "دروس تفاعلية", "Real-time collaboration": "تعاون في الوقت الحقيقي",
    "Student engagement tracking": "تتبع مشاركة الطلاب", "Our Features": "ميزاتنا",
    "This very extraordinary feature can make learning activities more efficient": "هذه الميزة تجعل الأنشطة التعليمية أكثر كفاءة",
    "A user interface designed for the classroom": "واجهة مصممة للفصل الدراسي",
    "Teachers and students can each customize their dashboard with settings they need most.": "يمكن تخصيص اللوحة حسب الاحتياجات.",
    "A user interface designed for the classroom that provides easy navigation for all users.": "واجهة سهلة التنقل لجميع المستخدمين.",
    "Quick access tools and organized dashboards make the learning environment modern.": "أدوات سريعة الوصول تجعل بيئة التعلم حديثة.",
    "Tools For Teachers And Learners": "أدوات للمعلمين والمتعلمين",
    "Class, has a dynamic set of teaching tools built to be deployed and used during class. Teachers can revitalize the lessons by using powerpoint, videos, and games.": "مجموعة ديناميكية من أدوات التدريس. يمكن للمعلمين إحياء الدروس بالعروض والفيديوهات.",
    "Explore more tools": "استكشف المزيد من الأدوات",
    "Assessments, Quizzes, Tests": "التقييمات والاختبارات",
    "Easily create and manage assessments to evaluate student performance. Set time limits, manage question banks, and get instant results for your class.": "أنشئ وأدر التقييمات بسهولة لتقييم أداء الطلاب.",
    "Class Management Tools for Educators": "أدوات إدارة الفصل",
    "Manage students, track progress, and organize classes seamlessly. Our tools give you total control over the learning environment with minimal effort.": "أدر الطلاب وتتبع التقدم بسهولة.",
    "One-on-One Discussions": "مناقشات فردية",
    "Enable meaningful conversations between students and instructors. Private messaging and video calls make personalized learning possible from anywhere.": "مناقشات فردية بين الطلاب والمعلمين.",
    "See more features": "شاهد المزيد من الميزات",
    "Explore Courses": "استكشف الدورات",
    "Data Science": "علم البيانات", "Creative Design": "التصميم الإبداعي",
    "UI/UX Design": "تصميم واجهات المستخدم", "Web Development": "تطوير الويب",
    "Marketing": "التسويق", "Business": "الأعمال",
    "What They Say?": "ماذا يقولون؟",
    "TOTC has got more than 100k positive ratings from our users around the world.": "TOTC حصلت على أكثر من 100 ألف تقييم إيجابي.",
    "Some of the students and teachers were greatly helped by the Skilline.": "ساعد النظام الكثير من الطلاب والمعلمين.",
    "Are you too? Please give your assessment": "هل أنت أيضاً؟ شاركنا تقييمك",
    "Write your assessment": "اكتب تقييمك",
    "Lastest News and Resources": "آخر الأخبار والموارد",
    "See the developments that have occurred to TOTC in the world": "تابع آخر التطورات التي شهدتها منصة TOTC.",
    "Read more": "اقرأ المزيد",
    "FAQ": "الأسئلة الشائعة", "Frequently Asked Questions": "الأسئلة الشائعة",
    "Everything you need to know about TOTC. Can't find the answer you're looking for? Reach out to our support team.": "كل ما تحتاج معرفته عن TOTC.",
    "Still have questions?": "لديك المزيد من الأسئلة؟",
    "Our support team is available 24/7 to help you with anything you need.": "فريق الدعم متاح على مدار الساعة.",
    "Contact Support": "تواصل مع الدعم",
    "Get In Touch": "تواصل معنا", "Register with TOTC": "سجّل مع TOTC",
    "Interested in joining TOTC or have a question? Fill out the form below and our team will reach out to you shortly.": "مهتم بالانضمام؟ أكمل النموذج وسيتواصل معك فريقنا.",
    "Why Choose TOTC?": "لماذا TOTC؟",
    "Expert Instructors": "مدرسون متخصصون",
    "Learn from qualified educators with real-world experience.": "تعلم من معلمين مؤهلين ذوي خبرة عملية.",
    "Safe & Monitored": "آمن ومراقب",
    "Parents stay informed with weekly progress reports.": "يبقى أولياء الأمور على اطلاع بتقارير أسبوعية.",
    "AI-Powered Learning": "تعلم مدعوم بالذكاء الاصطناعي",
    "Smart study tools, practice questions, and personalized plans.": "أدوات دراسة ذكية وأسئلة تدريبية وخطط مخصصة.",
    "Accessible Anywhere": "متاح في كل مكان",
    "Fully responsive — works on mobile, tablet, and desktop.": "يعمل على الجوال والتابلت والحاسوب.",
    "Full Name": "الاسم الكامل", "Email Address": "البريد الإلكتروني",
    "Phone Number": "رقم الهاتف", "Message / Inquiry": "الرسالة / الاستفسار",
    "Send Message": "إرسال الرسالة", "Done": "موافق", "Message Sent!": "تم الإرسال!",

    // ABOUT
    "Empowering Education Through Technology": "تمكين التعليم من خلال التكنولوجيا",
    "TOTC is a next-generation Learning Management System built to bring students, instructors, and parents together in one seamless platform.":"TOTC هو نظام إدارة التعلم من الجيل التالي، مصمم لجمع الطلاب والمعلمين وأولياء الأمور معًا في منصة واحدة سلسة.",
    "About TOTC": "عن TOTC", "Our Mission": "مهمتنا", "Our Vision": "رؤيتنا",
    "Our Goals": "أهدافنا", "Our Foundation": "أساسنا",
    "Mission, Vision & Goals": "المهمة والرؤية والأهداف",
    "To provide every student with access to high-quality, personalized education through an intelligent, easy-to-use digital platform — empowering learners at every level to reach their full potential.": "توفير تعليم عالي الجودة ومخصص لكل طالب عبر منصة رقمية ذكية.",
    "To become the leading educational technology platform in the region — a place where every school, instructor, and family can trust technology to make learning more effective, transparent, and joyful.": "أن نصبح المنصة التقنية التعليمية الرائدة في المنطقة.",
    "Bridge the gap between home and school. Equip instructors with AI-powered tools. Provide parents real-time visibility. Help students stay motivated with structured, gamified learning paths.": "ردم الفجوة بين البيت والمدرسة وتزويد المعلمين بأدوات ذكاء اصطناعي.",
    "Students Enrolled": "طلاب مسجلون", "Courses Available": "دورات متاحة",
    "Satisfaction Rate": "معدل الرضا",
    "Who We Are": "من نحن", "A Platform Built for Modern Education": "منصة للتعليم الحديث",
    "TOTC was born from a simple belief: every student deserves a structured, supportive, and smart learning environment. We combine the power of AI with intuitive design to give students, instructors, admins, and parents everything they need in one place.": "وُلد TOTC من قناعة بسيطة: كل طالب يستحق بيئة تعلم منظمة وداعمة وذكية.",
    "Our multi-role platform adapts to each user — from a student taking a timed exam, to a parent reading their child's weekly report, to an instructor generating AI-powered practice questions.": "منصتنا تتكيف مع كل مستخدم — من الطالب إلى المدرس إلى ولي الأمر.",
    "5 Role Dashboards": "5 لوحات تحكم", "AI-Powered Tools": "أدوات الذكاء الاصطناعي",
    "Weekly Parent Reports": "تقارير أسبوعية", "Real-time Notifications": "إشعارات فورية",
    "What We Offer": "ما نقدمه", "Our Services": "خدماتنا",
    "Course Management": "إدارة الدورات",
    "Full course lifecycle: create, publish, assign instructors, enroll students.": "دورة حياة الدورة الكاملة: إنشاء، نشر، تعيين مدرسين.",
    "AI Learning Tools": "أدوات التعلم بالذكاء الاصطناعي",
    "Chatbot, exam generator, study planner, key points, and MCQ practice.": "روبوت دردشة، مولد اختبارات، مخطط دراسة وأسئلة تدريبية.",
    "Multi-Role Access": "وصول متعدد الأدوار",
    "Separate dashboards for students, instructors, parents, admins, and super admins.": "لوحات تحكم منفصلة لكل دور.",
    "Automated Reporting": "تقارير آلية",
    "Weekly parent reports delivered via email every Friday, powered by n8n.": "تقارير أسبوعية لأولياء الأمور تُرسل كل جمعة.",
    "Support Tickets": "تذاكر الدعم",
    "Role-based ticketing with escalation from admin to superadmin.": "نظام تذاكر مع تصعيد من الإدارة.",
    "Mobile-First Design": "تصميم يبدأ من الجوال",
    "Fully responsive across all devices — phone, tablet, and desktop.": "متجاوب على جميع الأجهزة.",
    "Our Journey": "رحلتنا", "How TOTC Grew": "كيف نما TOTC",
    "Built for Everyone": "مبني للجميع",
    "Access courses, take exams, use AI tools": "الوصول للدورات، الاختبارات، الذكاء الاصطناعي",
    "Manage courses, grade, create with AI": "إدارة الدورات والتقييم وإنشاء المحتوى",
    "Admins": "المديرون", "Monitor all activity, manage users": "مراقبة الأنشطة وإدارة المستخدمين",
    "Super Admins": "المشرفون العامون", "Full system control, configuration": "تحكم كامل بالنظام",
    "Parents": "أولياء الأمور", "Monitor children, receive weekly reports": "متابعة الأبناء واستقبال التقارير",
    "2023": "2023", "The Idea": "الفكرة",
    "TOTC was conceived as a graduation project to solve real problems in digital education management.": "وُلدت فكرة TOTC كمشروع تخرج لحل مشكلات حقيقية في التعليم الرقمي.",
    "Early 2024": "مطلع 2024", "Core Platform Built": "بناء المنصة الأساسية",
    "Course management, exams, assignments, and multi-role dashboards launched.": "إطلاق إدارة الدورات والاختبارات والواجبات.",
    "Late 2024": "أواخر 2024", "AI Integration": "دمج الذكاء الاصطناعي",
    "RAG chatbot, exam generator, study planner, and automated parent reports added.": "إضافة روبوت الدردشة، مولد الاختبارات والتقارير الآلية.",
    "2025": "2025", "Full Platform Launch": "إطلاق المنصة الكاملة",
    "Complete system with n8n automation, notifications, ticketing, and mobile-first design.": "النظام الكامل مع أتمتة n8n والإشعارات ونظام التذاكر.",
    "Ready to Get Started?": "هل أنت مستعد للبدء؟",
    "Join thousands of students and educators already using TOTC. Contact us today.": "انضم إلى آلاف الطلاب والمعلمين. تواصل معنا اليوم.",
    "Get in Touch": "تواصل معنا",

    // GENERAL
    "Search": "بحث", "Filter": "تصفية", "Clear": "مسح", "Save": "حفظ",
    "Cancel": "إلغاء", "Delete": "حذف", "Edit": "تعديل", "Active": "نشط", "Inactive": "غير نشط",

    // HOME EXTRA
    "Studying": "التعلم",
    "Online is now much easier": "عبر الإنترنت أصبح أسهل بكثير",

    // TESTIMONIAL
    "Testimonial": "آراء المستخدمين",
    "Thank you so much for your help. It's exactly what I've been looking for. You won't regret it. It really saves me time and effort. TOTC is exactly what our business has been lacking.":
        "شكرًا جزيلًا لمساعدتكم. هذا بالضبط ما كنت أبحث عنه. لن تندم على استخدامه، فقد وفر عليّ الكثير من الوقت والجهد.",
    "12 reviews at Yelp": "12 تقييم على Yelp",

    // NEWS
    "NEWS": "أخبار",
    "PRESS RELEASE": "بيان صحفي",

    "Class adds $30 million to its balance sheet for a Zoom-friendly edtech solution":
        "أضافت Class مبلغ 30 مليون دولار لدعم حلول التعليم المتوافقة مع Zoom",

    "Class, launched less than a year ago by Blackboard co-founder Michael Chasen, integrates exclusively...":
        "تم إطلاق Class منذ أقل من عام بواسطة المؤسس المشارك لـ Blackboard...",

    "Class Technologies Inc. Closes $30 Million Series A Financing to Meet High Demand":
        "شركة Class Technologies تغلق جولة تمويل بقيمة 30 مليون دولار",

    "Class Technologies Inc., the company that created Class,...":
        "شركة Class Technologies، الشركة المطورة لمنصة Class...",

    "Zoom's earliest investors are betting millions on a better Zoom for schools":
        "أول مستثمري Zoom يراهنون بالملايين على نسخة تعليمية أفضل",

    "Zoom was never created to be a consumer product. Nonetheless, the...":
        "لم يتم إنشاء Zoom كمنتج استهلاكي في الأصل، ومع ذلك...",

    "Former Blackboard CEO Raises $16M to Bring LMS Features to Zoom Classrooms":
        "الرئيس التنفيذي السابق لـ Blackboard يجمع 16 مليون دولار لدعم التعليم عبر Zoom",

    "This year, investors have reaped big financial returns from betting on Zoom...":
        "حقق المستثمرون هذا العام أرباحًا كبيرة من الاستثمار في Zoom...",

    // FAQ QUESTIONS
    "What is TOTC and who is it for?":
        "ما هي منصة TOTC ولمن تم تصميمها؟",

    "TOTC (The Online Teaching Community) is a comprehensive learning management system designed for students, instructors, parents, and administrators. Whether you're a student looking to track your progress, a parent wanting to monitor your child's academic journey, or an instructor managing courses — TOTC has you covered.":
        "TOTC هي منصة تعليمية متكاملة مصممة للطلاب والمعلمين وأولياء الأمور والإداريين.",

    "How do I enroll in a course?":
        "كيف يمكنني التسجيل في دورة؟",

    "Course enrollment is managed by your school administrator. Once enrolled, you will automatically see the course appear in your student dashboard. Contact your admin or institution if you believe you should be enrolled in a course.":
        "يتم التسجيل في الدورات من خلال إدارة المؤسسة التعليمية. بعد التسجيل ستظهر الدورة تلقائياً في لوحة التحكم الخاصة بك.",

    "Can parents monitor their children's progress?":
        "هل يمكن لأولياء الأمور متابعة تقدم أبنائهم؟",

    "Yes! TOTC offers a dedicated Parent Portal where parents can view their children's enrolled courses, exam scores, assignment submissions, upcoming deadlines, and even download a weekly academic progress report in PDF format.":
        "نعم، توفر TOTC بوابة خاصة لأولياء الأمور لمتابعة أداء أبنائهم الدراسي.",

    "What types of exams does TOTC support?":
        "ما أنواع الاختبارات التي تدعمها TOTC؟",

    "TOTC supports multiple-choice question (MCQ) exams with a built-in question bank. Exams are timed, auto-scored, and display results immediately. Questions and answer choices are randomized for each student to maintain integrity.":
        "تدعم TOTC اختبارات الاختيار من متعدد مع تصحيح تلقائي وعرض فوري للنتائج.",

    "How are assignments submitted?":
        "كيف يتم تسليم الواجبات؟",

    "Students can submit assignments as text answers, file uploads (PDF, Word, images), or both — depending on how the instructor configured the assignment. Submissions are tracked, and late submissions are flagged automatically.":
        "يمكن للطلاب تسليم الواجبات كنصوص أو ملفات أو الاثنين معًا حسب إعدادات المدرس.",

    "Is TOTC accessible on mobile devices?":
        "هل تعمل TOTC على الهواتف المحمولة؟",

    "Absolutely. TOTC is fully responsive and works seamlessly on smartphones, tablets, and desktops. All dashboards adapt to your screen size for the best experience on any device.":
        "بالتأكيد، تعمل TOTC بسلاسة على الهواتف والأجهزة اللوحية وأجهزة الكمبيوتر.",

    // EXTRA BUTTONS
    "See more features": "عرض المزيد من الميزات",
    "Explore more tools": "استكشف المزيد من الأدوات",
    "Gloria Rose": "جلوريا روز"
};

// ═══════════════════════════ CORE ENGINE ══════════════════════════════
var currentLang = localStorage.getItem('totc_lang') || 'en';

function toggleLang() {
    currentLang = currentLang === 'en' ? 'ar' : 'en';
    localStorage.setItem('totc_lang', currentLang);
    applyLang();
}

function applyLang() {
    var isAr = currentLang === 'ar';
    document.documentElement.lang = isAr ? 'ar' : 'en';
    document.documentElement.dir = isAr ? 'rtl' : 'ltr';
    document.body.style.fontFamily = isAr
        ? "'Cairo','Segoe UI',sans-serif"
        : "'Poppins','Segoe UI',sans-serif";

    if (isAr && !document.getElementById('cairo-font')) {
        var lnk = document.createElement('link');
        lnk.id = 'cairo-font'; lnk.rel = 'stylesheet';
        lnk.href = 'https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700;800&display=swap';
        document.head.appendChild(lnk);
    }

    var flagEl = document.getElementById('langFlag');
    var labelEl = document.getElementById('langLabel');
    if (flagEl) flagEl.textContent = isAr ? '\uD83C\uDDEC\uD83C\uDDE7' : '\uD83C\uDDF8\uD83C\uDDE6';
    if (labelEl) labelEl.textContent = isAr ? 'English' : 'العربية';

    // PRIMARY: [data-i18n] elements
    document.querySelectorAll('[data-i18n]').forEach(function (el) {
        var key = el.dataset.i18n;
        if (isAr) {
            if (!el.dataset.orig) el.dataset.orig = el.textContent;
            // if (TRANSLATIONS[key]) el.textContent = TRANSLATIONS[key];
            if (TRANSLATIONS[key]) {
                if (!el.dataset.origHtml)
                    el.dataset.origHtml = el.innerHTML;

                el.innerHTML = TRANSLATIONS[key];
            }
        } else {
            // if (el.dataset.orig) el.textContent = el.dataset.orig;
            if (el.dataset.origHtml)
                el.innerHTML = el.dataset.origHtml;
        }
    });

    // SECONDARY: text-node fallback
    if (isAr) _walkAr();
    else _walkEn();

    // Placeholders
    document.querySelectorAll('[placeholder]').forEach(function (el) {
        if (isAr) {
            if (!el._origPh) el._origPh = el.getAttribute('placeholder');
            if (TRANSLATIONS[el._origPh]) el.setAttribute('placeholder', TRANSLATIONS[el._origPh]);
        } else {
            if (el._origPh) el.setAttribute('placeholder', el._origPh);
        }
    });

    // Navbar RTL
    var ms = document.querySelector('.ms-auto');
    if (ms) { ms.classList.toggle('ms-auto', !isAr); ms.classList.toggle('me-auto', isAr); }
}

var _saved = new Map();
function _walkAr() {
    var skip = new Set(['SCRIPT', 'STYLE', 'CODE', 'PRE', 'INPUT', 'TEXTAREA']);
    var w = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    var n;
    while ((n = w.nextNode())) {
        if (!n.parentElement || skip.has(n.parentElement.tagName)) continue;
        if (n.parentElement.closest('[data-i18n]')) continue;
        var t = n.textContent.trim();
        if (t.length < 2 || !TRANSLATIONS[t]) continue;
        if (!_saved.has(n)) _saved.set(n, n.textContent);
        n.textContent = n.textContent.replace(t, TRANSLATIONS[t]);
    }
}
function _walkEn() {
    _saved.forEach(function (v, n) { try { n.textContent = v; } catch (e) { } });
    _saved.clear();
}

document.addEventListener('DOMContentLoaded', function () {
    if (currentLang === 'ar') applyLang();
});