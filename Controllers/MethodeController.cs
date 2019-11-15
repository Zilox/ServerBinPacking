using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Projet.Controllers
{
    

    [Route("api/[controller]")]
    public class MethodeController : Controller
    {
        [HttpGet]
        [Route("{id}")]
        public int GetId(int id){
            return id;
        }

        
        [HttpPost]
        [Route("GetRandom/{alpha}/{nbObjet}")]
        public ActionResult getRandom(double alpha,int nbObjet)
        {
            DataMethod result = new DataMethod();
            Random r = new Random();
            double t = 0.00;

            result.result = new List<int>();
            result.timer = "";
            result.cout = 0;

            result.a = new List<int>();
            result.c = new List<int>();

            for(int i=0;i<nbObjet;i++)
            {
                result.a.Add(r.Next(10,100));
                result.c.Add(r.Next(10,100));
            }

            foreach (var item in result.a)
            {
                t += item;
            }

            result.b = alpha/100*t; 

            return Json(result);
        
        }

       
        [HttpPost]
        [Route("Calcul/")]
        
        public ActionResult getMethodeElementaire([FromBody] DataMethod item){
           
           Console.WriteLine("Traitement en cours");
           // Selon le numéro de méthode choisi on chois une option différente
            switch(item.numMethod)
            {
                case 1:
                    return Json(getMethodeEnumerationTotale(item));
                case 2:
                    return Json(getMethodeElementaireBranchBound(item));
                case 3:
                    return Json(getMethodeElementaireDynamiqueDense(item));
                case 4:
                    return Json(getMethodeGloutonne(item));
                case 5:
                    //return `Relaxation en continu`;
                    
                default:
                break;
            }

            Console.WriteLine("Traitement fini");
            return Json(item);
            
        }

        private object getMethodeGloutonne(DataMethod item)
        {

            // Variable utile pour le temps d'execution
            DateTime start = DateTime.Now;

            // on transforme nos tableau de int en double
            // Car on va faire des calcul de division
            double[] c = new double[item.c.Count];
            double[] a = new double[item.a.Count];
            int nb=0;

            // On calcule le total des poids
            foreach (var val in item.a)
            {
                a[nb] = (double)val;
                nb++;
            }

            nb=0;
            // On calcule le total des cout
            foreach (var val in item.c)
            {
                c[nb]=(double)val;
                nb++;   
            }

            //On recherche la valeur optimal inferieur
            item.bi = (int)get_inf_value(item,item.a.Count,item.b,c,a);


             // on transforme nos tableau de int en double
            // Car on va faire des calcul de division
            c = new double[item.c.Count];
            a = new double[item.a.Count];
            nb=0;
            foreach (var val in item.a)
            {
                a[nb] = (double)val;
                nb++;
            }
            nb=0;
            foreach (var val in item.c)
            {
                c[nb]=(double)val;
                nb++;   
            }

            //On recherche la valeur optimal inferieur
            item.bs = (int)get_sup_value(item,item.a.Count,item.b,c,a);
            
            // Fin du traitement on calcule le temps systeme passé
            TimeSpan dur = DateTime.Now - start;
            item.timer = dur.ToString();

            //on renvoie l'objet
            return item;
        }

        private double get_sup_value(DataMethod item,int n, double capacity, double[] values, double[] weights)
        {
            double value = 0.0;
            // On trie le tableau de valeurs et poids selon les valeurs
            Array.Sort(values, weights, Comparer<double>.Create((x, y) => y.CompareTo(x)));

            double[] ratio = new double[n];
            //on calcule le ration de chacun
            for (int i = 0; i < n; ++i)
            {
                ratio[i] = values[i] / weights[i];
            }

            //on range le ratio et le poids
            Array.Sort(ratio, weights, Comparer<double>.Create((x, y) => y.CompareTo(x)));
            
            //liste qui permet de restreindre a un élément le calcul
            List<int> conta = new List<int>();

            for (int i = 0; i < n; i++)
            {
                //Si la capicité vaut 0 alors on retour la valeur
                if (capacity == 0) return value;

                // On recupère le a si la capacité est suffisante
                double a = weights[i] < capacity ? weights[i] : capacity;

                if(a==capacity)
                {
                    value = value+ a*ratio[i];
                }
                // SI a n'est pas deja dans le contenu
                if(!conta.Contains((int)a))
                {
                    
                    conta.Add((int)a);

                    value = value + a * ratio[i];
                    weights[i] = weights[i] - a;

                    capacity = capacity - a;
                }
             
            }
            return value;
        }

        private double get_inf_value(DataMethod item,int n, double capacity, double[] values, double[] weights)
        {
            double value = 0.0;
            // On trie le tableau de valeurs et poids selon les valeurs
            Array.Sort(values, weights, Comparer<double>.Create((x, y) => y.CompareTo(x)));

            double[] ratio = new double[n];
            //on calcule le ration de chacun
            for (int i = 0; i < n; ++i)
            {
                ratio[i] = values[i] / weights[i];
            }

            //on range le ratio et le poids
            Array.Sort(ratio, weights, Comparer<double>.Create((x, y) => y.CompareTo(x)));
            
            //liste qui permet de restreind a un élément le calcul
            List<int> conta = new List<int>();

            for (int i = 0; i < n; i++)
            {
                //Si la capicité vaut 0 alors on retour la valeur
                if (capacity == 0) return value;

                // On recupère le a si la capacité est suffisante
                double a = weights[i] < capacity ? weights[i] : capacity;
                // SI a n'est pas deja dans le contenu
                if(!conta.Contains((int)a))
                {
                    // on rajoute dans la liste des resultat
                    item.result.Add(item.a.FindLastIndex(s=> s==(int)a));
                    conta.Add((int)a);

                    value = value + a * ratio[i];
                    weights[i] = weights[i] - a;

                    capacity = capacity - a;
                }
             
            }
            return value;
        }

        private DataMethod getMethodeElementaireDynamiqueDense(DataMethod item)
        {

            DateTime start = DateTime.Now;

             int[] m = new int[(int)item.b+1];

            for(int i=0;i<item.a.Count;i++)
                for(int j=(int)item.b;j>=0;j--)
                {
                    m[j] = j < item.a[i] ? m[j] : Math.Max(m[j],m[j-item.a[i]] + item.c[i]);
                } 

            item.cout = m[(int)item.b];



            TimeSpan dur = DateTime.Now - start;
            item.timer = dur.ToString();


            return item;
        }


        private DataMethod getMethodeElementaireBranchBound(DataMethod item)
        {
            
            DateTime start = DateTime.Now;
            
            item.cout = sousArbre(item.b,item.a,item.c,item.a.Count, item.result);

            TimeSpan dur = DateTime.Now - start;
            item.timer = dur.ToString();

            return item;
        }

        public int sousArbre(double W, List<int> wt, List<int> val, int n, List<int> index)
        {
                        
            if (n == 0 || W == 0)
                return 0;
 
            if (wt[n-1] > W)
                return sousArbre(W, wt, val, n-1,index);
 
             else{
                 var max = Math.Max( val[n-1] + sousArbre(W-wt[n-1], wt, val, n-1,index),
                    sousArbre(W, wt, val, n-1,index)
                  );
                 
                 return  max;
             } 
        }

        
        ///==========================================================================
        // Methode Enumeration Totale (ATTENTION METHODE TRES GOURMANDE)
        // Cette méthode va resoudre notre solution de façon binaire
        // Elle va mettre toute les possibilités en binaire (1 indiquant que l'objet sera présent et 0 non)
        // De la on va prendre la solution max qui ne dépasse pas le poids
        ///==========================================================================
        public DataMethod getMethodeEnumerationTotale(DataMethod item)
        {
            item.result = new List<int>();
            DateTime start = DateTime.Now;
            
            // On va se creer la liste des possibilités qui sera ben binaire
            List<List<int>> possibilites = new List<List<int>>();
            
            // On est donc a 2 puissance nbObjet possible 
            for(int i=0;i < (Math.Pow(2,item.a.Count));i++)
            {
                String binary = Convert.ToString(i,2); // On convertie notre nombre en binaire
                int Size = binary.Length;
            
               // On rajoute les 0 du début binaire (Pour la phase de calcul)
                for(int j=0;j<(item.a.Count - Size);j++)
                    binary = "0"+binary;

                List<int> subList = new List<int>();
               
                for(int k=0;k<item.a.Count;k++ )
                {
                    if(binary[k] =='1')
                        subList.Add(k);
                }  
                 possibilites.Add(subList);
                 
            }

            int max=0;

            // On cherche parmi toute nos possibilités
            // La cout max qui ne depasse pas le poid
            foreach (var sub in possibilites)
            {
                
                int result=0;
                int cout=0;

                foreach (var number in sub)
                {
                   
                    result += item.a[number];
                    cout += item.c[number];
                }

                // Si le resultate est inferieur a la capacité max du sac
                // et le cout est maximisé
                if(result< item.b && cout>max)
                {
                  
                    max = cout;
                    item.cout = cout;
                    item.result = sub;
                }
            }

            // Calcul du temps calcul
            TimeSpan dur = DateTime.Now - start;
            item.timer = dur.ToString();

            return item;
        }

      
    }

    


    public class DataMethod
    {
        // Durée de l'execution
        public String timer;

        // Valeur du cout de B
        public double b;
        
        // Liste des poids des objets (A trier dans l'ordre des numéros d'objet)
        public  List<int> a ;

        // Liste des couts des objets (A trier dans l'ordre des numéros d'objet)
        public  List<int> c ;

        
        public  List<int> result; // Liste de la solution optimal

        
        public int cout;// Cout total enregistrer

        
        public int bi; // Borne inférieur

        public int bs;        // Borne superieur
        
        public int numMethod; // Numero de Methode
        
    }

}